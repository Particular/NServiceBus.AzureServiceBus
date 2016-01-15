namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class DefaultBrokeredMessagesToIncomingMessagesConverter : IConvertBrokeredMessagesToIncomingMessages
    {
        readonly ReadOnlySettings settings;

        public DefaultBrokeredMessagesToIncomingMessagesConverter(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public IncomingMessageDetails Convert(BrokeredMessage brokeredMessage)
        {
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
                        throw new ConfigurationErrorsException(errorMessage, e);
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
                    throw new ConfigurationErrorsException("Unsupported brokered message body type configured");
            }
            

            if (!string.IsNullOrWhiteSpace(brokeredMessage.ReplyTo) && !headers.ContainsKey(Headers.ReplyToAddress))
            {
                headers[Headers.ReplyToAddress] = brokeredMessage.ReplyTo;
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

        private string GetDefaultTransportEncoding()
        {
            var configuredDefault = settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType);
            return configuredDefault == SupportedBrokeredMessageBodyTypes.ByteArray ? "wcf/byte-array" : "application/octect-stream";
        }
    }
}