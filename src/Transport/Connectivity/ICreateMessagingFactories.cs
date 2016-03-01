namespace NServiceBus.AzureServiceBus
{
    public interface ICreateMessagingFactories
    {
        IMessagingFactory Create(string namespaceName);
    }
}