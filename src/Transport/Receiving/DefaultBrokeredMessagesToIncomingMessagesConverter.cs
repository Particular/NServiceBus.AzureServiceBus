namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using Topology.MetaModel;
    using Logging;
    using Settings;

    class DefaultBrokeredMessagesToIncomingMessagesConverter : IConvertBrokeredMessagesToIncomingMessages
    {
        ILog logger = LogManager.GetLogger<DefaultBrokeredMessagesToIncomingMessagesConverter>();
        ReadOnlySettings settings;
        ICanMapConnectionStringToNamespaceName mapper;

        public DefaultBrokeredMessagesToIncomingMessagesConverter(ReadOnlySettings settings, ICanMapConnectionStringToNamespaceName mapper)
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

            Stream rawBody;
            switch (transportEncodingToUse)
            {
                case "wcf/byte-array":
                    try
                    {
                        rawBody = new MemoryStream(brokeredMessage.GetBody<byte[]>() ?? new byte[0]);
                    }
                    catch (Exception e)
                    {
                        var errorMessage = transportEncodingWasSpecified ? $"Unsupported brokered message body type `${transportEncodingToUse}` configured" : "No brokered message body type was found. Attempt to process message body as byte array has failed.";
                        throw new UnsupportedBrokeredMessageBodyTypeException(errorMessage, e);
                    }
                    break;
                case "application/octect-stream":
                    rawBody = new MemoryStream();
                    using (var body = brokeredMessage.GetBody<Stream>())
                    {
                        body.CopyTo(rawBody);
                        rawBody.Position = 0;
                    }
                    break;
                default:
                    throw new UnsupportedBrokeredMessageBodyTypeException("Unsupported brokered message body type configured");
            }

            var replyToHeaderValue = headers.ContainsKey(Headers.ReplyToAddress) ?
                headers[Headers.ReplyToAddress] : brokeredMessage.ReplyTo;

            if (!string.IsNullOrWhiteSpace(replyToHeaderValue))
            {
                headers[Headers.ReplyToAddress] = mapper.Map(replyToHeaderValue);
            }

            if (!string.IsNullOrWhiteSpace(brokeredMessage.CorrelationId) && !headers.ContainsKey(Headers.CorrelationId))
            {
                headers[Headers.CorrelationId] = brokeredMessage.CorrelationId;
            }

            if (brokeredMessage.TimeToLive < TimeSpan.MaxValue && !headers.ContainsKey(Headers.TimeToBeReceived))
            {
                headers[Headers.TimeToBeReceived] = brokeredMessage.TimeToLive.ToString("c", CultureInfo.InvariantCulture);
            }

            return new IncomingMessageDetails(brokeredMessage.MessageId, headers, rawBody);
        }

        string GetDefaultTransportEncoding()
        {
            var configuredDefault = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            return configuredDefault == SupportedBrokeredMessageBodyTypes.ByteArray ? "wcf/byte-array" : "application/octect-stream";
        }
    }
}