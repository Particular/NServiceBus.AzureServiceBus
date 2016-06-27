namespace NServiceBus.Transport.AzureServiceBus
{
    public class EntityRelationShipInfo
    {
        public EntityInfo Source { get; set; }
        public EntityInfo Target { get; set; }
        public EntityRelationShipType Type { get; set; }
    }
}