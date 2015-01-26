namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Threading;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;

    class AzureServiceBusQueueNotifier : INotifyReceivedBrokeredMessages
    {
        Action<BrokeredMessage> tryProcessMessage;
        bool cancelRequested;

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueNotifier));
        
        public QueueClient QueueClient { get; set; }

        public int ServerWaitTime { get; set; }
        public int BatchSize { get; set; }
        public int BackoffTimeInSeconds { get; set; }

        public Type MessageType { get; set; }
        public Address Address { get; set; }

        Action<Exception> errorProcessingMessage;

        public event EventHandler Faulted;

        void OnFaulted()
        {
            if (Faulted != null)
                Faulted(this, EventArgs.Empty);
        }

        public void Start(Action<BrokeredMessage> tryProcessMessage, Action<Exception> errorProcessingMessage)
        {
            cancelRequested = false;

            this.tryProcessMessage = tryProcessMessage;
            this.errorProcessingMessage = errorProcessingMessage;

            SafeBeginReceive();
        }

        public void Stop()
        {
            cancelRequested = true;
        }

        void OnMessage(IAsyncResult ar)
        {
            try
            {
                if (QueueClient.IsClosed)
                {
                    Stop();
                    OnFaulted();
                    return;
                }

                var receivedMessages = QueueClient.EndReceiveBatch(ar);

                if (cancelRequested) return;

                foreach (var receivedMessage in receivedMessages)
                {
                    tryProcessMessage(receivedMessage);
                }
            }
            catch (TimeoutException ex)
            {
                // time's up, just continue and retry
                logger.Warn(string.Format("Timeout Exception occured on queue {0}", QueueClient.Path), ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Fatal(string.Format("Unauthorized Access Exception occured on queue {0}", QueueClient.Path), ex);

                errorProcessingMessage(ex);
            }
            catch (MessagingException ex)
            {
                if (cancelRequested)
                {
                    return;
                }

                if (!ex.IsTransient && !RetriableReceiveExceptionHandling.IsRetryable(ex))
                {
                    logger.Fatal(string.Format("{1} {2} occured on queue {0}", QueueClient.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);

                    errorProcessingMessage(ex);
                }
                else
                {
                    logger.Warn(string.Format("{1} {2} occured on queue {0}", QueueClient.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                }


                logger.Warn("Will retry after backoff period");

                Thread.Sleep(TimeSpan.FromSeconds(BackoffTimeInSeconds));
            }
            catch (OperationCanceledException ex)
            {
                logger.Fatal(string.Format("Operation cancelled exception occured on receive for queue {0}, faulting this notifier", QueueClient.Path), ex);
                Stop();
                OnFaulted();
            }
            finally
            {
                SafeBeginReceive();
            }
        }

        void SafeBeginReceive()
        {
            if (!cancelRequested)
            {
                try
                {
                    QueueClient.BeginReceiveBatch(BatchSize, TimeSpan.FromSeconds(ServerWaitTime), OnMessage, null);
                }
                catch (OperationCanceledException ex)
                {
                    logger.Fatal(string.Format("Operation cancelled exception occured on receive for queue {0}, faulting this notifier", QueueClient.Path), ex);
                    Stop();
                    OnFaulted();
                }
            }
        }
    }
}