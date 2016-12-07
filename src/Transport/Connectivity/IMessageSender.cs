namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    interface IMessageSenderInternal : IClientEntityInternal
    {
        Task Send(BrokeredMessage message);

        Task SendBatch(IEnumerable<BrokeredMessage> messages);
    }
}