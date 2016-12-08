namespace NServiceBus.Transport.AzureServiceBus
{
    interface IManageMessageReceiverLifeCycleInternal
    {
        IMessageReceiverInternal Get(string entityPath, string namespaceAlias);
    }
}