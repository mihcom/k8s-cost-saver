using k8s.Models;
using KubeOps.Operator.Events;

namespace CostSaver.Extensions;

public static class EventManagerExtensions
{
    public static async Task PublishNamespaceExpiredEvent(this IEventManager eventManager, Entities.CostSaver costSaver, V1Namespace @namespace)
    {
        var message = $"Namespace {@namespace.Metadata.Name} expired (created at {@namespace.Metadata.CreationTimestamp} UTC, " +
                      $"{costSaver.Spec.NamespaceLabel} is {@namespace.Metadata.Labels[costSaver.Spec.NamespaceLabel]}";

        await eventManager.PublishAsync(costSaver, "NamespaceExpired", message);
    }
    
    public static async Task PublishNamespaceDeletedEvent(this IEventManager eventManager, Entities.CostSaver costSaver, V1Namespace @namespace)
    {
        var message = $"Namespace {@namespace.Metadata.Name} has been deleted";

        await eventManager.PublishAsync(costSaver, "NamespaceDeleted", message);
    }

    public static async Task PublishGenericErrorEvent(this IEventManager eventManager, Entities.CostSaver costSaver, string operation, Exception ex)
    {
        var message = $"An unhandled error occurred while performing {operation}: {ex.Message} at {ex.StackTrace}";
        
        await eventManager.PublishAsync(costSaver, "NamespaceDeleted", message);
    }
}
