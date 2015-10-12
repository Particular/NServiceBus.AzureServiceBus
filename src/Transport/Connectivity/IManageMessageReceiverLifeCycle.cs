namespace NServiceBus.AzureServiceBus
{
    public interface IManageMessageReceiverLifeCycle
    {
        IMessageReceiver Get(string entitypath, string connectionstring);
    }
}