namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class BrokeredMessagesToIncomingMessagesConverter
    {
        public BrokeredMessagesToIncomingMessagesConverter(ReadOnlySettings settings, DefaultConnectionStringToNamespaceAliasMapper mapper)
        {
            this.mapper = mapper;
            defaultTransportEncoding = GetDefaultTransportEncoding(settings);
        }

        public IncomingMessageDetailsInternal Convert(BrokeredMessage brokeredMessage)
        {
            if (!brokeredMessage.Properties.ContainsKey(BrokeredMessageHeaders.TransportEncoding))
            {
                logger.Debug($"Incoming BrokeredMessage with id=`{brokeredMessage.MessageId}` had no `{BrokeredMessageHeaders.TransportEncoding}` header.");
            }


            var transportEncodingWasSpecified = brokeredMessage.Properties.ContainsKey(BrokeredMessageHeaders.TransportEncoding);
            var transportEncodingToUse = transportEncodingWasSpecified ? brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] as string : defaultTransportEncoding;

            byte[] body;
            switch (transportEncodingToUse)
            {
                case "wcf/byte-array":
                    try
                    {
                        body = brokeredMessage.GetBody<byte[]>() ?? EmptyBody;
                    }
                    catch (Exception e)
                    {
                        var errorMessage = transportEncodingWasSpecified ? $"Unsupported brokered message body type `${transportEncodingToUse}` configured" : "No brokered message body type was found. Attempt to process message body as byte array has failed.";
                        throw new UnsupportedBrokeredMessageBodyTypeException(errorMessage, e);
                    }
                    break;
                case "application/octet-stream":
                case "application/octect-stream":
                    var bodyStream = brokeredMessage.GetBody<Stream>();
                    body = new byte[bodyStream.Length];
                    // TODO : This could be async
                    bodyStream.Read(body, 0, (int)bodyStream.Length);
                    break;
                default:
                    throw new UnsupportedBrokeredMessageBodyTypeException("Unsupported brokered message body type configured");
            }

            return new IncomingMessageDetailsInternal(brokeredMessage.MessageId, GetHeaders(brokeredMessage), body);
        }

        public Dictionary<string, string> GetHeaders(BrokeredMessage brokeredMessage)
        {
            var headers = brokeredMessage.Properties
                .Where(kvp => kvp.Key != BrokeredMessageHeaders.TransportEncoding)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);

            var replyToHeaderValue = headers.ContainsKey(Headers.ReplyToAddress) ? headers[Headers.ReplyToAddress] : brokeredMessage.ReplyTo;

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

            return headers;
        }

        static string GetDefaultTransportEncoding(ReadOnlySettings settings)
        {
            var configuredDefault = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            return configuredDefault == SupportedBrokeredMessageBodyTypes.ByteArray ? "wcf/byte-array" : "application/octect-stream";
        }

        ILog logger = LogManager.GetLogger<BrokeredMessagesToIncomingMessagesConverter>();
        DefaultConnectionStringToNamespaceAliasMapper mapper;
        string defaultTransportEncoding;
        static byte[] EmptyBody = new byte[0];
    }
}