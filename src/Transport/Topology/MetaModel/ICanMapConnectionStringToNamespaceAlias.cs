namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICanMapConnectionStringToNamespaceAlias
    {
        EntityAddress Map(EntityAddress value);
    }
}
