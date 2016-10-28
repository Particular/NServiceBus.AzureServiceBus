namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessagingFactory : IClientEntity
    {
        Task<IMessageReceiver> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode);

        Task<IMessageSender> CreateMessageSender(string entitypath);

        Task<IMessageSender> CreateMessageSender(string entitypath, string viaEntityPath);

        Task CloseAsync();
    }
}