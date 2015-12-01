namespace NServiceBus.AzureServiceBus
{
    public interface IManageMessagingFactoryLifeCycle
    {
        IMessagingFactory Get(string @namespace);
    }
}