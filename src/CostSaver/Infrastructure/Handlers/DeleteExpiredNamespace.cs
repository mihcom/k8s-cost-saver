using CostSaver.Extensions;
using CostSaver.Infrastructure.Events;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Events;

namespace CostSaver.Infrastructure.Handlers;

public class DeleteExpiredNamespace(IEventManager eventManager, IKubernetesClient kubernetesClient, 
    ILogger<DeleteExpiredNamespace> logger) : INotificationHandler<ExpiredNamespaceDetectedEvent>
{
    public async Task Handle(ExpiredNamespaceDetectedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await kubernetesClient.Delete(notification.Namespace);
            logger.NamespaceDeleted(notification.Namespace.Metadata.Name);
            await eventManager.PublishNamespaceDeletedEvent(notification.CostSaver, notification.Namespace);
        }
        catch (Exception e)
        {
            logger.GenericError("Namespace deletion", e);
            await eventManager.PublishGenericErrorEvent(notification.CostSaver, $"Deleting namespace {notification.Namespace.Metadata.Name} failed", e); 
        }
    }
}
