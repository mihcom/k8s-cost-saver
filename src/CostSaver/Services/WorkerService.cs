using k8s.Models;
using KubeOps.KubernetesClient;

namespace CostSaver.Services;

public class WorkerService(IKubernetesClient kubernetesClient, ILogger<WorkerService> logger) : IHostedService
{
    // Safety net to prevent accidental deletion of important namespaces
    private static readonly HashSet<string> ProtectedNamespaces =
        new() { "default", "ingress-nginx", "kube-node-lease", "kube-public", "kube-system", "logging" };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(10);

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
        logger.LogInformation("Checking cost saver {CostSaverName}", costSaver.Metadata.Name);

        var expiredNamespaces = namespaces
                .Where(ns => ns.Metadata.CreationTimestamp.HasValue 
                        && ns.Metadata.Labels.TryGetValue(costSaver.Spec.NamespaceLabel, out var labelValue)
                        && !string.IsNullOrWhiteSpace(labelValue)
                        && TimeSpan.TryParse(labelValue.Replace('-', ':'), out var expirationTime)
                        && ns.Metadata.CreationTimestamp.Value.Add(expirationTime) < DateTime.UtcNow);

        await Parallel.ForEachAsync(expiredNamespaces, cancellationToken, async (expiredNamespace, _) =>
        {
            logger.LogInformation("Deleting expired namespace {NamespaceName} (created at {NamespaceCreated}, {NamespaceLabel} is {LabelValue})", 
                expiredNamespace.Metadata.Name, expiredNamespace.Metadata.CreationTimestamp!.Value, costSaver.Spec.NamespaceLabel, expiredNamespace.Metadata.Labels[costSaver.Spec.NamespaceLabel]);
            
            await kubernetesClient.Delete(expiredNamespace);
        });
    }

    private async Task<IEnumerable<V1Namespace>> GetNamespaces()
    {
        var allNamespaces = await kubernetesClient.List<V1Namespace>();

        var allowedNamespaces = allNamespaces
            .Where(ns => !ProtectedNamespaces.Contains(ns.Metadata.Name))
            .ToArray();

        return allowedNamespaces;
    }

    private async Task<IEnumerable<Entities.CostSaver>> GetCostSavers()
    {
        return await kubernetesClient.List<Entities.CostSaver>();
    }
}
