namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessageReceiverLifeCycle
    {
        IMessageReceiver Get(string entityPath, string namespaceAlias);
    }
}