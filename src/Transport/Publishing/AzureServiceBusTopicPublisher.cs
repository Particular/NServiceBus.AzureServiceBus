namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Threading;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;

    class AzureServiceBusTopicPublisher : IPublishBrokeredMessages
    {
        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusTopicPublisher));

        public TopicClient TopicClient { get; set; }

        public const int DefaultBackoffTimeInSeconds = 10;
        public int MaxDeliveryCount { get; set; }

        public void Publish(BrokeredMessage brokeredMessage)
        {
            var toSend = brokeredMessage;
            var numRetries = 0;
            var sent = false;

            while (!sent)
            {
                try
                {
                    TopicClient.Send(toSend);
                   
                    sent = true;
                }
                // todo, outbox
                catch (MessagingEntityDisabledException)
                {
                    logger.Warn(string.Format("Topic {0} is disabled", TopicClient.Path)); 

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();
                }
                // back off when we're being throttled
                catch (ServerBusyException ex)
                {
                    logger.Warn(string.Format("Server busy exception occured on topic {0}", TopicClient.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();
                }
                // took to long, maybe we lost connection
                catch (TimeoutException ex)
                {
                    logger.Warn(string.Format("Timeout exception occured on topic {0}", TopicClient.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();
                }
                // connection lost
                catch (MessagingCommunicationException ex)
                {
                    logger.Warn(string.Format("Messaging Communication Exception occured on topic {0}", TopicClient.Path), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();
                }
                catch (MessagingException ex)
                {
                    logger.Warn(string.Format("{1} Messaging Exception occured on topic {0}", TopicClient.Path, (ex.IsTransient ? "Transient" : "Non transient")), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount || !ex.IsTransient) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();
                }
            }
        }

        
    }
}