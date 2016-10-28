namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class TopologySection
    {
        public IEnumerable<RuntimeNamespaceInfo> Namespaces { get; set; }
        public IEnumerable<EntityInfo> Entities { get; set; }
        public IEnumerable<EntityRelationShipInfo> Relationships { get; set; }
    }
}