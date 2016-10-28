namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessageReceivers
    {
        Task<IMessageReceiver> Create(string entityPath, string namespaceAlias);
    }
}