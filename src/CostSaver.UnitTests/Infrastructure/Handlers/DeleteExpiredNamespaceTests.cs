using CostSaver.Infrastructure.Events;
using CostSaver.Infrastructure.Handlers;

namespace CostSaver.UnitTests.Infrastructure.Handlers;

public class DeleteExpiredNamespaceTests
{
    private readonly IEventManager _eventManager;
    private readonly DeleteExpiredNamespace _handler;
    private readonly IKubernetesClient _kubernetesClient;

    public DeleteExpiredNamespaceTests()
    {
        _eventManager = Substitute.For<IEventManager>();
        _kubernetesClient = Substitute.For<IKubernetesClient>();
        var logger = Substitute.For<ILogger<DeleteExpiredNamespace>>();
        _handler = new DeleteExpiredNamespace(_eventManager, _kubernetesClient, logger);
    }

    [Theory]
    [AutoData]
    public async Task Handle_ShouldDeleteNamespace_WhenNamespaceIsExpired(ExpiredNamespaceDetectedEvent @event)
    {
        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _kubernetesClient.Received(1).Delete(@event.Namespace);
    }

    [Theory]
    [AutoData]
    public async Task Handle_ShouldPublishNamespaceDeletedEvent_WhenNamespaceIsDeleted(ExpiredNamespaceDetectedEvent @event)
    {
        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventManager.Received(1)
            .PublishAsync(@event.CostSaver, "NamespaceDeleted", Arg.Is<string>(s => s.Contains(@event.Namespace.Metadata.Name)));
    }

    [Theory]
    [AutoData]
    public async Task Handle_ShouldPublishGenericErrorEvent_WhenExceptionIsThrown(ExpiredNamespaceDetectedEvent @event, Exception exception)
    {
        // Arrange
        _kubernetesClient
            .When(k => k.Delete(@event.Namespace))
            .Do(x => throw exception);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _eventManager.Received(1).PublishAsync(@event.CostSaver,
            "GenericError", Arg.Is<string>(s => s.Contains(exception.Message) && s.Contains(exception.StackTrace!)));
    }
}
