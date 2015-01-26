using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Logging;
    using NServiceBus.Transports;
    using Settings;
    using Unicast.Queuing;

    /// <summary>
    /// 
    /// </summary>
    public class AzureServiceBusMessageQueueSender : ISendMessages
    {
        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusMessageQueueSender));

        const int DefaultBackoffTimeInSeconds = 10;

        private static readonly Dictionary<string, QueueClient> senders = new Dictionary<string, QueueClient>();
        
        private static readonly object SenderLock = new Object();

        public int MaxDeliveryCount { get; set; }

        ICreateMessagingFactories createMessagingFactories;

        public AzureServiceBusMessageQueueSender(ICreateMessagingFactories createMessagingFactories)
        {
            this.createMessagingFactories = createMessagingFactories;
        }

        public void Send(TransportMessage message, string destination)
        {
            Send(message, Address.Parse(destination));
        }

        public void Send(TransportMessage message, Address address)
        {
            var destination = address.Queue;
            var @namespace = address.Machine;

            QueueClient sender;
            if (!senders.TryGetValue(destination, out sender))
            {
                lock (SenderLock)
                {
                    if (!senders.TryGetValue(destination, out sender))
                    {
                        var factory = createMessagingFactories.Create(@namespace);
                        sender = factory.CreateQueueClient(destination);
                        senders[destination] = sender;
                    }
                }
            }

            if (!SettingsHolder.Get<bool>("Transactions.Enabled") || Transaction.Current == null)
                Send(message, sender,address);
            else
                Transaction.Current.EnlistVolatile(new SendResourceManager(() => Send(message, sender, address)), EnlistmentOptions.None);

        }

        void Send(TransportMessage message, QueueClient sender, Address address)
        {
            var numRetries = 0;
            var sent = false;

            while (!sent)
            {
                try
                {
                    using (var brokeredMessage = message.Body != null ? new BrokeredMessage(message.Body) : new BrokeredMessage())
                    {
                        brokeredMessage.CorrelationId = message.CorrelationId;
                        if (message.TimeToBeReceived < TimeSpan.MaxValue) brokeredMessage.TimeToLive = message.TimeToBeReceived;

                        foreach (var header in message.Headers)
                        {
                            brokeredMessage.Properties[header.Key] = header.Value;
                        }

                        brokeredMessage.Properties[Headers.MessageIntent] = message.MessageIntent.ToString();
                        brokeredMessage.MessageId = message.Id;

                        if (message.ReplyToAddress != null)
                        {
                            brokeredMessage.ReplyTo = new DeterminesBestConnectionStringForAzureServiceBus().Determine(message.ReplyToAddress);
                        }

                        if (message.TimeToBeReceived < TimeSpan.MaxValue)
                        {
                            brokeredMessage.TimeToLive = message.TimeToBeReceived;
                        }

                        if (brokeredMessage.Size > 256*1024)
                        {
                            throw new MessageTooLargeException(string.Format("The message with id {0} is larger that the maximum message size allowed by Azure ServiceBus, consider using the databus instead", message.Id));
                        }

                        sender.Send(brokeredMessage);
                        sent = true;
                    }
                }
                catch (MessagingEntityNotFoundException)
                {
                    throw new QueueNotFoundException
                    {
                        Queue = address
                    };
                }
                    // todo: outbox
                catch (MessagingEntityDisabledException)
                {
                    logger.Warn(string.Format("Queue {0} is disable", sender.Path));

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries*DefaultBackoffTimeInSeconds));
                }
                    // back off when we're being throttled
                catch (ServerBusyException ex)
                {
                    logger.Warn(string.Format("Server busy exception occured on queue {0}", sender.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries*DefaultBackoffTimeInSeconds));
                }
                    // connection lost
                catch (MessagingCommunicationException ex)
                {
                    logger.Warn(string.Format("Messaging Communication exception occured on queue {0}", sender.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries*DefaultBackoffTimeInSeconds));
                }
                    // took to long, maybe we lost connection
                catch (TimeoutException ex)
                {
                    logger.Warn(string.Format("Timeout exception occured on queue {0}", sender.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries*DefaultBackoffTimeInSeconds));
                }

                catch (MessagingException ex)
                {
                    logger.Warn(string.Format("{1} Messaging exception occured on subscription {0}", sender.Path, (ex.IsTransient ? "Transient" : "Non transient")), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount || !ex.IsTransient) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));
                }
            }
        }
   }
}