using k8s.Models;
using KubeOps.Operator.Entities;

namespace CostSaver.Entities;

[KubernetesEntity(Group = "costsaver.kubeops.io", ApiVersion = "v1")]
public class CostSaver : CustomKubernetesEntity<CostSaverSpec, CostSaverStatus>
{
}

public class CostSaverSpec
{
    public string NamespaceLabel { get; set; } = null!;
}

public class CostSaverStatus
{
}