namespace NServiceBus.Transport.AzureServiceBus
{
    class EntityRelationShipInfoInternal
    {
        public EntityInfoInternal Source { get; set; }
        public EntityInfoInternal Target { get; set; }
        public EntityRelationShipTypeInternal Type { get; set; }
    }
}