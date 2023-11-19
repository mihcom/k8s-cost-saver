using k8s.Models;

namespace CostSaver.Infrastructure.Events;

public record ExpiredNamespaceDetectedEvent(Entities.CostSaver CostSaver, V1Namespace Namespace) : INotification;
