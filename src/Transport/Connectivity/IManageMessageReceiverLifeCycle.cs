namespace NServiceBus.Transport.AzureServiceBus
{
    public interface IManageMessageReceiverLifeCycle
    {
        IMessageReceiver Get(string entityPath, string namespaceAlias);
    }
}