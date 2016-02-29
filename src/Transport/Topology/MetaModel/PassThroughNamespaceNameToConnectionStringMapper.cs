namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    class PassThroughNamespaceNameToConnectionStringMapper : ICanMapNamespaceNameToConnectionString
    {
        public EntityAddress Map(EntityAddress value)
        {
            return new EntityAddress(value);
        }
    }
}