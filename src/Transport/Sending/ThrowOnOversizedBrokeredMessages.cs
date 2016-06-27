namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class ThrowOnOversizedBrokeredMessages : IHandleOversizedBrokeredMessages
    {
        public Task Handle(BrokeredMessage brokeredMessage)
        {
            throw new MessageTooLargeException($"The message with id {brokeredMessage.MessageId} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus feature.");
        }
    }
}