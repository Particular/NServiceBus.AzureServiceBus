namespace NServiceBus.Transport.AzureServiceBus
{
    public interface IManageMessageReceiverLifeCycle
    {
        IMessageReceiver Get(string entitypath, string namespaceName);
    }
}