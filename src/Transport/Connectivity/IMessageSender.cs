namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IMessageSender : IClientEntity
    {
        Task SendAsync(BrokeredMessage message);

        Task SendBatchAsync(IEnumerable<BrokeredMessage> messages);
    }
}