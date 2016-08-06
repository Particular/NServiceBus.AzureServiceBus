namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IHandleOversizedBrokeredMessages
    {
        Task Handle(BrokeredMessage brokeredMessage);
    }
}