namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICanMapNamespaceNameToConnectionString
    {
        EntityAddress Map(EntityAddress value);
    }
}