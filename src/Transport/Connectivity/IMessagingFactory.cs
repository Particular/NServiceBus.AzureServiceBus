namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IMessagingFactory : IClientEntity
    {
        Task<IMessageReceiver> CreateMessageReceiver(string entitypath, ReceiveMode receiveMode);

        Task<IMessageSender> CreateMessageSender(string entitypath);

        Task<IMessageSender> CreateMessageSender(string entitypath, string viaEntityPath);
    }
}