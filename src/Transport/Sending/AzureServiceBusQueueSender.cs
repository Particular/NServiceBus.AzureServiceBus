namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Threading;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;

    class AzureServiceBusQueueSender : ISendBrokeredMessages
    {
        const int DefaultBackoffTimeInSeconds = 10;

        public int MaxDeliveryCount { get; set; }

        public QueueClient QueueClient { get; set; }

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueSender));

        public void Send(BrokeredMessage brokeredMessage)
        {
            var toSend = brokeredMessage;
            var numRetries = 0;
            var sent = false;

            while (!sent)
            {
                try
                {
                   QueueClient.Send(toSend);
                    
                   sent = true;
                }
                // took to long, maybe we lost connection
                catch (TimeoutException ex)
                {
                    logger.Warn(string.Format("{1} occured when sending to queue {0}", QueueClient.Path, ex.GetType().Name), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries*DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();

                }
                //The operation cannot be performed because the brokered message '...' has already been consumed. Please use a new BrokeredMessage instance for the operation.
                catch (InvalidOperationException ex)
                {
                    logger.Warn(string.Format("{1} occured when sending to queue {0}", QueueClient.Path, ex.GetType().Name), ex);

                    numRetries++;

                    if (numRetries >= MaxDeliveryCount) throw;

                    logger.Warn("Will retry after backoff period");

                    Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                    toSend = toSend.CloneWithMessageId();

                }
                // connection lost
                catch (MessagingException ex)
                {
                    if (!ex.IsTransient && !RetriableSendExceptionHandling.IsRetryable(ex))
                    {
                        logger.Fatal(string.Format("{1} {2} occured when sending to queue {0}", QueueClient.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);

                        throw;
                    }
                    else
                    {
                        logger.Warn(string.Format("{1} {2} occured when sending to queue {0}", QueueClient.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);

                        numRetries++;

                        if (numRetries >= MaxDeliveryCount) throw;

                        logger.Warn("Will retry after backoff period");

                        Thread.Sleep(TimeSpan.FromSeconds(numRetries * DefaultBackoffTimeInSeconds));

                        toSend = toSend.CloneWithMessageId();
                    }
                }
            }
        }
    }
}