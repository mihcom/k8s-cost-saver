using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace CostSaver.Entities;

[KubernetesEntity(Group = "costsaver.leapwork", ApiVersion = "v1")]
public class CostSaver : CustomKubernetesEntity<CostSaverSpec, CostSaverStatus>
{
}

[Description("The cost saver specification.")]
public class CostSaverSpec
{
    [Description("The namespace label to track.")]
    public string NamespaceLabel { get; set; } = null!;
}

[Description("The cost saver status.")]
public class CostSaverStatus
{
    [Description("The tracked namespaces.")]
    public IEnumerable<ExpiringNamespace> TrackedNamespaces { get; set; } = Enumerable.Empty<ExpiringNamespace>();

    [Description("The tracked namespace details.")]
    public class ExpiringNamespace
    {
        [Description("The name of the namespace.")]
        public string Name { get; set; } = null!;

        [Description("The expiration date of the namespace.")]
        public DateTime ExpiresAt { get; set; }
    }
}
