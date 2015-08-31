namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IMessagingFactory : IClientEntity
    {
        Task<IMessageReceiver> CreateMessageReceiverAsync(string entitypath, ReceiveMode receiveMode);

        Task<IMessageSender> CreateMessageSenderAsync(string entitypath);
    }
}