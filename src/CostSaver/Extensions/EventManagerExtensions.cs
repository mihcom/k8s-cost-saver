using k8s.Models;
using KubeOps.Operator.Events;

namespace CostSaver.Extensions;

public static class EventManagerExtensions
{
    public static async Task PublishExpiredNamespaceEvent(this IEventManager eventManager, Entities.CostSaver costSaver, V1Namespace @namespace)
    {
        var message = $"Namespace {@namespace.Metadata.Name} expired (created at {@namespace.Metadata.CreationTimestamp} UTC, " +
                      $"{costSaver.Spec.NamespaceLabel} is {@namespace.Metadata.Labels[costSaver.Spec.NamespaceLabel]}";

        await eventManager.PublishAsync(costSaver, "NamespaceExpired", message);
    }
}
