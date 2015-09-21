namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;

    public class TopologyDefinition
    {
        public IEnumerable<NamespaceInfo> Namespaces { get; set; }
        public IEnumerable<EntityInfo> Entities { get; set; }
        public IEnumerable<EntityRelationShipInfo> Relationships { get; set; }
    }
}