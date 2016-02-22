namespace NServiceBus.AzureServiceBus
{
    public interface IManageMessageSenderLifeCycle
    {
        IMessageSender Get(string entitypath, string viaEntityPath, string namespaceName);
    }
}