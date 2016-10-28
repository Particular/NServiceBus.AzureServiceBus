namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessageSenderLifeCycle
    {
        IMessageSender Get(string entitypath, string viaEntityPath, string namespaceName);
    }
}