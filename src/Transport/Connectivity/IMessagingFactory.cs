namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    interface IMessagingFactoryInternal : IClientEntityInternal
    {
        Task<IMessageReceiverInternal> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode);

        Task<IMessageSenderInternal> CreateMessageSender(string entitypath);

        Task<IMessageSenderInternal> CreateMessageSender(string entitypath, string viaEntityPath);

        Task CloseAsync();
    }
}