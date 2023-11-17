using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;

namespace CostSaver.Controllers;

public class CostSaveController : IResourceController<Entities.CostSaver>
{
    private readonly ILogger<CostSaveController> _logger;

    public CostSaveController(ILogger<CostSaveController> logger)
    {
        _logger = logger;
    }

    public Task<ResourceControllerResult?> ReconcileAsync(Entities.CostSaver entity)
    {
        _logger.LogInformation("Reconciling {EntityName} in namespace {Namespace}.", entity.Metadata.Name, entity.Metadata.NamespaceProperty);
        return Task.FromResult((ResourceControllerResult?)null);
    }

    public Task DeletedAsync(Entities.CostSaver entity)
    {
        _logger.LogInformation("Deleting {EntityName} in namespace {Namespace}.", entity.Metadata.Name, entity.Metadata.NamespaceProperty);
        return Task.CompletedTask;
    }
}
