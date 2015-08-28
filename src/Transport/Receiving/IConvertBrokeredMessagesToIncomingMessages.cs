namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Transports;

    public interface IConvertBrokeredMessagesToIncomingMessages
    {
        IncomingMessage Convert(BrokeredMessage brokeredMessage);
    }

    class DefaultBrokeredMessagesToIncomingMessagesConverter : IConvertBrokeredMessagesToIncomingMessages
    {
        public IncomingMessage Convert(BrokeredMessage brokeredMessage)
        {
            return new IncomingMessage("", new Dictionary<string, string>(), new MemoryStream() );
        }
    }
}