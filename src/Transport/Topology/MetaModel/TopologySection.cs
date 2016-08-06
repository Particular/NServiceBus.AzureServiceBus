namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    public class TopologySection
    {
        public IEnumerable<RuntimeNamespaceInfo> Namespaces { get; set; }
        public IEnumerable<EntityInfo> Entities { get; set; }
        public IEnumerable<EntityRelationShipInfo> Relationships { get; set; }
    }
}