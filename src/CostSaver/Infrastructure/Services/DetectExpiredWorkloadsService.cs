using System.Collections.Concurrent;
using CostSaver.Extensions;
using CostSaver.Infrastructure.Events;
using k8s.Models;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Events;

namespace CostSaver.Infrastructure.Services;

public class DetectExpiredWorkloadsService(IKubernetesClient kubernetesClient, IEventManager eventManager,
    IPublisher mediator, ILogger<DetectExpiredWorkloadsService> logger) : IHostedService
{
    // Safety net to prevent accidental deletion of important namespaces
    private static readonly HashSet<string> ProtectedNamespaces =
        new() { "default", "ingress-nginx", "kube-node-lease", "kube-public", "kube-system", "logging" };

    // Keep track of processed namespaces to prevent duplicate events
    private static readonly ConcurrentBag<string> ProcessedNamespaces = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        #if DEBUG
        var interval = TimeSpan.FromSeconds(10);
        #else
        var interval = TimeSpan.FromMinutes(1);
        #endif

        while (!cancellationToken.IsCancellationRequested)
        {
            await CheckForExpiredNamespaces(cancellationToken);
            await Task.Delay(interval, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CheckForExpiredNamespaces(CancellationToken cancellationToken)
    {
        var getNamespacesTask = GetNamespaces();
        var getCostSaversTask = GetCostSavers();

        await Task.WhenAll(getNamespacesTask, getCostSaversTask);

        cancellationToken.ThrowIfCancellationRequested();

        var namespaces = getNamespacesTask.Result;
        var costSavers = getCostSaversTask.Result;

        await Parallel.ForEachAsync(costSavers, cancellationToken, (costSaver, ct) => CheckCostSaver(costSaver, namespaces, ct));
    }

    private async ValueTask CheckCostSaver(Entities.CostSaver costSaver, IEnumerable<V1Namespace> namespaces, CancellationToken cancellationToken)
    {
        logger.CheckingCostSaver(costSaver.Metadata.Name);

        var expiredNamespaces = namespaces
            .Where(ns => ns.Metadata.CreationTimestamp.HasValue
                         && ns.Metadata.Labels.TryGetValue(costSaver.Spec.NamespaceLabel, out var labelValue)
                         && !string.IsNullOrWhiteSpace(labelValue)
                         && TimeSpan.TryParse(labelValue.Replace('-', ':'), out var expirationTime)
                         && ns.Metadata.CreationTimestamp.Value.Add(expirationTime) < DateTime.UtcNow);

        await Parallel.ForEachAsync(expiredNamespaces, cancellationToken,
            async (expiredNamespace, ct) =>
            {
                ProcessedNamespaces.Add(GetNamespaceKey(expiredNamespace));

                logger.NamespaceExpired(expiredNamespace.Metadata.Name);
                await eventManager.PublishNamespaceExpiredEvent(costSaver, expiredNamespace);
                await mediator.Publish(new ExpiredNamespaceDetectedEvent(costSaver, expiredNamespace), ct);
            });
    }

    private async Task<IEnumerable<V1Namespace>> GetNamespaces()
    {
        var allNamespaces = await kubernetesClient.List<V1Namespace>();

        var allowedNamespaces = allNamespaces
            .Where(ns => !ProtectedNamespaces.Contains(ns.Metadata.Name))
            .Where(ns => !ProcessedNamespaces.Contains(GetNamespaceKey(ns)))
            .ToArray();

        return allowedNamespaces;
    }

    private async Task<IEnumerable<Entities.CostSaver>> GetCostSavers()
    {
        return await kubernetesClient.List<Entities.CostSaver>();
    }

    private static string GetNamespaceKey(V1Namespace @namespace)
    {
        return $"{@namespace.Metadata.Name}:{@namespace.Metadata.CreationTimestamp}";
    }
}
