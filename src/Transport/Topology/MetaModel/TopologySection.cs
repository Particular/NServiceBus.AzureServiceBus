namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    class TopologySectionInternal
    {
        public IEnumerable<RuntimeNamespaceInfo> Namespaces { get; set; }
        public IEnumerable<EntityInfoInternal> Entities { get; set; }
        public IEnumerable<EntityRelationShipInfoInternal> Relationships { get; set; }
    }
}