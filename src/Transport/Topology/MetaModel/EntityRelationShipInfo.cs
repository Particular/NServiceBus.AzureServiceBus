namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EntityRelationShipInfo
    {
        public EntityInfo Source { get; set; }
        public EntityInfo Target { get; set; }
        public EntityRelationShipType Type { get; set; }
    }
}