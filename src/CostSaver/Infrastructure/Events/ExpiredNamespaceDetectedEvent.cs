using k8s.Models;

namespace CostSaver.Infrastructure.Events;

public record ExpiredNamespaceDetectedEvent(V1Namespace Namespace) : INotification;
