namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICreateMessagingFactoriesInternal
    {
        IMessagingFactoryInternal Create(string namespaceName);
    }
}