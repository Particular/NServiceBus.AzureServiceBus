namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class DefaultBrokeredMessagesToIncomingMessagesConverter : IConvertBrokeredMessagesToIncomingMessages
    {
        ILog logger = LogManager.GetLogger<DefaultBrokeredMessagesToIncomingMessagesConverter>();
        ReadOnlySettings settings;
        DefaultConnectionStringToNamespaceAliasMapper mapper;
        static byte[] EmptyBody = new byte[0];

        public DefaultBrokeredMessagesToIncomingMessagesConverter(ReadOnlySettings settings, DefaultConnectionStringToNamespaceAliasMapper mapper)
        {
            this.settings = settings;
            this.mapper = mapper;
        }

        public IncomingMessageDetails Convert(BrokeredMessage brokeredMessage)
        {
            if (!brokeredMessage.Properties.ContainsKey(BrokeredMessageHeaders.TransportEncoding))
            {
                logger.Debug($"Incoming BrokeredMessage with id=`{brokeredMessage.MessageId}` had no `{BrokeredMessageHeaders.TransportEncoding}` header.");
            }

            var headers = brokeredMessage.Properties
                .Where(kvp => kvp.Key != BrokeredMessageHeaders.TransportEncoding)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);

            var transportEncodingWasSpecified = brokeredMessage.Properties.ContainsKey(BrokeredMessageHeaders.TransportEncoding);
            var transportEncodingToUse = transportEncodingWasSpecified ? brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] as string : GetDefaultTransportEncoding();

            var body = EmptyBody;

            var bodyStream = brokeredMessage.GetBody<Stream>();
            if (bodyStream != null)
            {
                try
                {
                    body = new byte[bodyStream.Length];
                    // TODO : This could be async
                    bodyStream.Read(body, 0, (int)bodyStream.Length);
                }
                catch (Exception e)
                {
                    var error = transportEncodingWasSpecified
                        ? $"Supported brokered message body type `${transportEncodingToUse}` was found, but couldn't read message body. See internal exception for details."
                        : "No brokered message body type was found. Attempt to process message body has failed.";
                    throw new UnsupportedBrokeredMessageBodyTypeException(error, e);
                }
            }

            var replyToHeaderValue = headers.ContainsKey(Headers.ReplyToAddress) ?
                headers[Headers.ReplyToAddress] : brokeredMessage.ReplyTo;

            if (!string.IsNullOrWhiteSpace(replyToHeaderValue))
            {
                headers[Headers.ReplyToAddress] = mapper.Map(new EntityAddress(replyToHeaderValue)).ToString();
            }

            if (!string.IsNullOrWhiteSpace(brokeredMessage.CorrelationId) && !headers.ContainsKey(Headers.CorrelationId))
            {
                headers[Headers.CorrelationId] = brokeredMessage.CorrelationId;
            }

            if (brokeredMessage.TimeToLive < TimeSpan.MaxValue && !headers.ContainsKey(Headers.TimeToBeReceived))
            {
                headers[Headers.TimeToBeReceived] = brokeredMessage.TimeToLive.ToString("c", CultureInfo.InvariantCulture);
            }

            return new IncomingMessageDetails(brokeredMessage.MessageId, headers, body);
        }

        string GetDefaultTransportEncoding()
        {
            var configuredDefault = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            return configuredDefault == SupportedBrokeredMessageBodyTypes.ByteArray ? "wcf/byte-array" : "application/octect-stream";
        }
    }
}