namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Linq;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Unicast;

    static class BrokeredMessageConverter
    {
        public static TransportMessage ToTransportMessage(this BrokeredMessage message)
        {
            TransportMessage t;
            var rawMessage = BrokeredMessageBodyConversion.ExtractBody(message);

            if (message.Properties.Count > 0)
            {
                var headers = message.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string);
                if (!String.IsNullOrWhiteSpace(message.ReplyTo))
                {
                    headers[Headers.ReplyToAddress] = message.ReplyTo;
                }

                t = new TransportMessage(message.MessageId, headers)
                {
                    CorrelationId = message.CorrelationId,
                    TimeToBeReceived = message.TimeToLive,
                    MessageIntent = (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), message.Properties[Headers.MessageIntent].ToString()),
                    Body = rawMessage
                };
            }
            else
            {
                t = new TransportMessage
                {
                    Body = rawMessage
                };
            }

            return t;
        }

        public static BrokeredMessage ToBrokeredMessage(this TransportMessage message, PublishOptions options, ReadOnlySettings settings, Configure config)
        {
            var brokeredMessage = BrokeredMessageBodyConversion.InjectBody(message.Body);

            SetHeaders(message, options, settings, config, brokeredMessage);

            if (message.TimeToBeReceived < TimeSpan.MaxValue)
            {
                brokeredMessage.TimeToLive = message.TimeToBeReceived;
            }

            GuardMessageSize(brokeredMessage);

            return brokeredMessage;
        }

        public static BrokeredMessage ToBrokeredMessage(this TransportMessage message, SendOptions options, SettingsHolder settings, bool expectDelay, Configure config)
        {
            var brokeredMessage = BrokeredMessageBodyConversion.InjectBody(message.Body);

            SetHeaders(message, options, settings, config, brokeredMessage);

            var timeToSend = DelayIfNeeded(options, expectDelay);
                        
            if (timeToSend.HasValue)
                brokeredMessage.ScheduledEnqueueTimeUtc = timeToSend.Value;

            TimeSpan? timeToLive = null;
            if (message.TimeToBeReceived < TimeSpan.MaxValue)
            {
                timeToLive = message.TimeToBeReceived;
            }
            else if (options.TimeToBeReceived.HasValue && options.TimeToBeReceived < TimeSpan.MaxValue)
            {
                timeToLive = options.TimeToBeReceived.Value;
            }

            if (timeToLive.HasValue)
            {
                if (timeToLive.Value <= TimeSpan.Zero) return null;

                brokeredMessage.TimeToLive = timeToLive.Value;
            }
            GuardMessageSize(brokeredMessage);

            return brokeredMessage;
        }

        static void GuardMessageSize(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage.Size > 256*1024)
            {
                throw new MessageTooLargeException(string.Format("The message with id {0} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus instead", brokeredMessage.MessageId));
            }
        }

        static void SetHeaders(TransportMessage message, DeliveryOptions options, ReadOnlySettings settings, Configure config, BrokeredMessage brokeredMessage)
        {
            foreach (var header in message.Headers)
            {
                brokeredMessage.Properties[header.Key] = header.Value;
            }

            brokeredMessage.Properties[Headers.MessageIntent] = message.MessageIntent.ToString();
            brokeredMessage.MessageId = message.Id;
            brokeredMessage.CorrelationId = message.CorrelationId;

            if (message.ReplyToAddress != null)
            {
                brokeredMessage.ReplyTo = new DeterminesBestConnectionStringForAzureServiceBus(config.TransportConnectionString()).Determine(settings, message.ReplyToAddress);
            }
            else if (options.ReplyToAddress != null)
            {
                brokeredMessage.ReplyTo = new DeterminesBestConnectionStringForAzureServiceBus(config.TransportConnectionString()).Determine(settings, options.ReplyToAddress);
            }
        }

        static DateTime? DelayIfNeeded(SendOptions options, bool expectDelay)
        {
            DateTime? deliverAt = null;

            if (options.DelayDeliveryWith.HasValue)
            {
                deliverAt = DateTime.UtcNow + options.DelayDeliveryWith.Value;
            }
            else
            {
                if (options.DeliverAt.HasValue)
                {
                    deliverAt = options.DeliverAt.Value;
                }
                else if (expectDelay)
                {
                    throw new ArgumentException("A delivery time needs to be specified for Deferred messages");
                }

            }

            return deliverAt;
        }
    }

    public static class BrokeredMessageBodyConversion
    {
        public static Func<BrokeredMessage, byte[]> ExtractBody = message => message.GetBody<byte[]>() ?? new byte[0];
        public static Func<byte[], BrokeredMessage> InjectBody = bytes => bytes != null ? new BrokeredMessage(bytes) : new BrokeredMessage();
    }
}