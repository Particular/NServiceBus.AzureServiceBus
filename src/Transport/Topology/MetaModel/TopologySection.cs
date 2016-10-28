namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class TopologySection
    {
        public IEnumerable<RuntimeNamespaceInfo> Namespaces { get; set; }
        public IEnumerable<EntityInfo> Entities { get; set; }
        public IEnumerable<EntityRelationShipInfo> Relationships { get; set; }
    }
}