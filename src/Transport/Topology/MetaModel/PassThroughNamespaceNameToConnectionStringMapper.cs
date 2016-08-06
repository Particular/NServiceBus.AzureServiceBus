namespace NServiceBus.Transport.AzureServiceBus
{
    class PassThroughNamespaceNameToConnectionStringMapper : ICanMapNamespaceNameToConnectionString
    {
        public EntityAddress Map(EntityAddress value)
        {
            return value;
        }
    }
}