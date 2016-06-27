namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICanMapConnectionStringToNamespaceName
    {
        EntityAddress Map(EntityAddress value);
    }
}
