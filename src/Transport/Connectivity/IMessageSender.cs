namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IMessageSender : IClientEntity
    {
        Task Send(BrokeredMessage message);

        Task SendBatch(IEnumerable<BrokeredMessage> messages);
    }
}