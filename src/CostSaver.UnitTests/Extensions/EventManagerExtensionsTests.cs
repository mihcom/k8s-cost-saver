namespace CostSaver.UnitTests.Extensions;

public class EventManagerExtensionsTests
{
    [Theory]
    [AutoData]
    public async Task PublishNamespaceExpiredEvent_ShouldPublishEvent(CostSaverEntity costSaver, V1Namespace @namespace)
    {
        // Arrange
        var eventManager = Substitute.For<IEventManager>();
        var fixture = new Fixture();
        @namespace.Metadata.Labels.Add(costSaver.Spec.NamespaceLabel, fixture.Create<string>("namespace-label"));

        // Act
        await eventManager.PublishNamespaceExpiredEvent(costSaver, @namespace);

        // Assert
        await eventManager.Received().PublishAsync(
            Arg.Is(costSaver),
            Arg.Is("NamespaceExpired"),
            Arg.Is<string>(s =>
                s.Contains(@namespace.Metadata.Name) && s.Contains(@namespace.Metadata.CreationTimestamp.ToString()!) &&
                s.Contains(@namespace.Metadata.Labels[costSaver.Spec.NamespaceLabel])));
    }

    [Theory]
    [AutoData]
    public async Task PublishNamespaceDeletedEvent_ShouldPublishEvent(CostSaverEntity costSaver, V1Namespace @namespace)
    {
        // Arrange
        var eventManager = Substitute.For<IEventManager>();
        var fixture = new Fixture();
        @namespace.Metadata.Labels.Add(costSaver.Spec.NamespaceLabel, fixture.Create<string>("namespace-label"));

        // Act
        await eventManager.PublishNamespaceDeletedEvent(costSaver, @namespace);

        // Assert
        await eventManager.Received().PublishAsync(
            Arg.Is(costSaver),
            Arg.Is("NamespaceDeleted"),
            Arg.Is<string>(s => s.Contains(@namespace.Metadata.Name)));
    }
    
    [Theory]
    [AutoData]
    public async Task PublishGenericErrorEvent_ShouldPublishEvent(CostSaverEntity costSaver, string operation, Exception ex)
    {
        // Arrange
        var eventManager = Substitute.For<IEventManager>();
        var expectedMessage = $"An unhandled error occurred while performing {operation}: {ex.Message} at {ex.StackTrace}";

        // Act
        await eventManager.PublishGenericErrorEvent(costSaver, operation, ex);

        // Assert
        await eventManager.Received().PublishAsync(
            Arg.Is(costSaver),
            Arg.Is("GenericError"),
            Arg.Is(expectedMessage));
    }
}
