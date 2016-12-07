namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICreateMessagingFactoriesInternal
    {
        IMessagingFactory Create(string namespaceName);
    }
}