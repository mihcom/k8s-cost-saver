using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;
using KubeOps.Operator.Rbac;

namespace CostSaver.Entities;

[KubernetesEntity(Group = "costsaver.leapwork", ApiVersion = "v1")]
[EntityRbac(typeof(V1Namespace), Verbs = RbacVerb.List | RbacVerb.Delete)]
[EntityRbac(typeof(CostSaver), Verbs = RbacVerb.All)]
public class CostSaver : CustomKubernetesEntity<CostSaverSpec, CostSaverStatus>
{
}

[Description("The cost saver specification.")]
public class CostSaverSpec
{
    [Description("The namespace label to track.")]
    public string NamespaceLabel { get; init; } = null!;
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
        public required string Name { get; init; }

        [Description("The creation timestamp of the namespace.")]
        public required DateTime CreatedAt { get; init; }

        [Description("The lifetime of the namespace.")]
        public required string Lifetime { get; init; }
        
        [Description("The expiration date of the namespace.")]
        public DateTime ExpiresAt { get; init; }
    }
}
