using CostSaver.Infrastructure.Events;
using CostSaver.Infrastructure.Services;
using MediatR;

namespace CostSaver.UnitTests.Infrastructure.Services;

public class DetectExpiredWorkloadsServiceTests
{
    private readonly IEventManager _eventManager;
    private readonly IKubernetesClient _kubernetesClient;
    private readonly IPublisher _mediator;
    private readonly DetectExpiredWorkloadsService _service;

    public DetectExpiredWorkloadsServiceTests()
    {
        _kubernetesClient = Substitute.For<IKubernetesClient>();
        _eventManager = Substitute.For<IEventManager>();
        _mediator = Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<DetectExpiredWorkloadsService>>();
        _service = new DetectExpiredWorkloadsService(_kubernetesClient, _eventManager, _mediator, logger);
    }

    [Fact]
    public async Task StartAsync_ShouldCheckForExpiredNamespaces()
    {
        // Act
        _ = _service.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        await _kubernetesClient.Received(1).List<V1Namespace>();
        await _kubernetesClient.Received(1).List<CostSaverEntity>();
    }

    [Theory]
    [AutoData]
    public async Task CheckForExpiredNamespaces_ShouldUpdateCostSaverStatus(IList<CostSaverEntity> costSavers, IList<V1Namespace> namespaces)
    {
        // Arrange
        _kubernetesClient.List<V1Namespace>().Returns(namespaces);
        _kubernetesClient.List<CostSaverEntity>().Returns(costSavers);

        // Act
        _ = _service.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        await Parallel.ForEachAsync(costSavers, CancellationToken.None, async (costSaver, _) =>
            await _kubernetesClient.Received(1).UpdateStatus(Arg.Is(costSaver)));
    }

    [Theory]
    [AutoData]
    public async Task CheckForExpiredNamespaces_ShouldPublishNamespaceExpiredEvent_WhenNamespaceIsExpired(IList<CostSaverEntity> costSavers,
        IList<V1Namespace> namespaces)
    {
        // Arrange
        _kubernetesClient.List<V1Namespace>().Returns(namespaces);
        _kubernetesClient.List<CostSaverEntity>().Returns(costSavers);

        // each cost saver controls a namespace
        const string expireInOneSecond = "00-00-01";

        for (var i = 0; i < costSavers.Count; i++)
        {
            var costSaver = costSavers[i];
            var @namespace = namespaces[i];

            @namespace.Metadata.CreationTimestamp = DateTime.UtcNow;
            @namespace.Metadata.Labels.Add(costSaver.Spec.NamespaceLabel, expireInOneSecond);
        }

        // Act
        // wait for namespaces to expire
        await Task.Delay(TimeSpan.FromSeconds(3));
        _ = _service.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        for (var i = 0; i < costSavers.Count; i++)
        {
            var costSaver = costSavers[i];
            var @namespace = namespaces[i];

            await _eventManager.Received(1)
                .PublishAsync(costSaver, "NamespaceExpired", Arg.Is<string>(s => s.Contains(@namespace.Metadata.Name)));
        }
    }

    [Theory]
    [AutoData]
    public async Task CheckForExpiredNamespaces_ShouldPublishExpiredNamespaceDetectedEvent_WhenNamespaceIsExpired(IList<CostSaverEntity> costSavers,
        IList<V1Namespace> namespaces)
    {
        // Arrange
        _kubernetesClient.List<V1Namespace>().Returns(namespaces);
        _kubernetesClient.List<CostSaverEntity>().Returns(costSavers);

        // each cost saver controls a namespace
        const string expireInOneSecond = "00-00-01";

        for (var i = 0; i < costSavers.Count; i++)
        {
            var costSaver = costSavers[i];
            var @namespace = namespaces[i];

            @namespace.Metadata.CreationTimestamp = DateTime.UtcNow;
            @namespace.Metadata.Labels.Add(costSaver.Spec.NamespaceLabel, expireInOneSecond);
        }

        // Act
        // wait for namespaces to expire
        await Task.Delay(TimeSpan.FromSeconds(3));
        _ = _service.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        for (var i = 0; i < costSavers.Count; i++)
        {
            var costSaver = costSavers[i];
            var @namespace = namespaces[i];

            await _mediator.Received(1)
                .Publish(Arg.Is<ExpiredNamespaceDetectedEvent>(@event => @event.CostSaver == costSaver && @event.Namespace == @namespace),
                    Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [AutoData]
    public async Task GetNamespaces_ShouldFilterOutProtectedNamespaces(IList<V1Namespace> namespaces)
    {
        // Arrange
        var protectedNamespaces =
            DetectExpiredWorkloadsService.ProtectedNamespaces.Select(x => new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    Name = x
                }
            })
            .ToArray();
        
        var mixedNamespaces = namespaces.Concat(protectedNamespaces).ToList();
        _kubernetesClient.List<V1Namespace>().Returns(mixedNamespaces);

        // Act
        var resultNamespaces = (await _service.GetNamespaces()).ToArray();

        // Assert
        resultNamespaces.Length.Should().Be(namespaces.Count);
        
        foreach (var protectedNamespace in protectedNamespaces)
        {
            resultNamespaces.Should().NotContain(x => x.Metadata.Name == protectedNamespace.Metadata.Name);
        }

        foreach (var expectedNamespace in namespaces)
        {
            resultNamespaces.Should().Contain(x => x.Metadata.Name == expectedNamespace.Metadata.Name);
        }
    }
}
