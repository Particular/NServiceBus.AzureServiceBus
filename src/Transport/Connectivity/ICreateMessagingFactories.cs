namespace NServiceBus.Transport.AzureServiceBus
{
    public interface ICreateMessagingFactories
    {
        IMessagingFactory Create(string namespaceName);
    }
}