namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Oversized <see cref="BrokeredMessage"/> handling strategy to throw and exception.
    /// </summary>
    public class ThrowOnOversizedBrokeredMessages : IHandleOversizedBrokeredMessages
    {
        /// <summary>
        /// <remarks>Throws and exception with recommendation to use DataBus feature.</remarks>
        /// </summary>
        public Task Handle(BrokeredMessage brokeredMessage)
        {
            throw new MessageTooLargeException($"The message with id {brokeredMessage.MessageId} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus feature.");
        }
    }
}