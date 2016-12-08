namespace NServiceBus.Transport.AzureServiceBus
{
    interface IManageMessageSenderLifeCycleInternal
    {
        IMessageSenderInternal Get(string entitypath, string viaEntityPath, string namespaceName);
    }
}