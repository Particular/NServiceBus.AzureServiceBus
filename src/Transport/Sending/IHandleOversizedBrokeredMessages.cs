namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Contract to implement custom strategy to handle oversized <see cref="BrokeredMessage"/>.
    /// </summary>
    public interface IHandleOversizedBrokeredMessages
    {
        /// <summary></summary>
        Task Handle(BrokeredMessage brokeredMessage);
    }
}