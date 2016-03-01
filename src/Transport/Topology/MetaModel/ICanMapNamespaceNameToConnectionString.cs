namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    interface ICanMapNamespaceNameToConnectionString
    {
        EntityAddress Map(EntityAddress value);
    }
}