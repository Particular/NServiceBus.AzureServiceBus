namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    interface ICanMapConnectionStringToNamespaceName
    {
        EntityAddress Map(EntityAddress value);
    }
}
