namespace NServiceBus.AzureServiceBus
{
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class DefaultBrokeredMessagesToIncomingMessagesConverter : IConvertBrokeredMessagesToIncomingMessages
    {
        readonly ReadOnlySettings settings;

        public DefaultBrokeredMessagesToIncomingMessagesConverter(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public IncomingMessage Convert(BrokeredMessage brokeredMessage)
        {
            var headers = brokeredMessage.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);

            Stream rawBody;
            var bodyType = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            switch (bodyType)
            {
                case SupportedBrokeredMessageBodyTypes.ByteArray:
                    rawBody = new MemoryStream(brokeredMessage.GetBody<byte[]>() ?? new byte[0]);
                    break;
                case SupportedBrokeredMessageBodyTypes.Stream:
                    rawBody = new MemoryStream();
                    using (var body = brokeredMessage.GetBody<Stream>())
                    {
                        body.CopyTo(rawBody);
                        rawBody.Position = 0;
                    }
                    break;
                default:
                    throw new ConfigurationErrorsException("Unsupported brokered message body type configured");
            }
            var incomingMessage = new IncomingMessage(brokeredMessage.MessageId, headers, rawBody);

            return incomingMessage;
        }
    }

    public enum SupportedBrokeredMessageBodyTypes
    {
        ByteArray,
        Stream
    }
}