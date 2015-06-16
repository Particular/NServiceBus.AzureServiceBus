namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;

    class AzureServiceBusQueueCreator : Transports.ICreateQueues
    {
        public TimeSpan LockDuration { get; set; }
        public long MaxSizeInMegabytes { get; set; }
        public bool RequiresDuplicateDetection { get; set; }
        public bool RequiresSession { get; set; }
        public TimeSpan DefaultMessageTimeToLive { get; set; }
        public bool EnableDeadLetteringOnMessageExpiration { get; set; }
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; }
        public int MaxDeliveryCount { get; set; }
        public bool EnableBatchedOperations { get; set; }
        public bool EnablePartitioning { get; set; }
        public bool SupportOrdering { get; set; }

        ICreateNamespaceManagers createNamespaceManagers;
        Configure config;

        static ConcurrentDictionary<string, bool> rememberExistence = new ConcurrentDictionary<string, bool>();

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueCreator));

        public AzureServiceBusQueueCreator(ICreateNamespaceManagers createNamespaceManagers, Configure config)
        {
            this.createNamespaceManagers = createNamespaceManagers;
            this.config = config;
        }

        public QueueDescription Create(Address address)
        {
            var queueName = address.Queue;
            var path = "";
            var namespaceClient = createNamespaceManagers.Create(address.Machine);

            var description = new QueueDescription(queueName)
            {
                LockDuration = LockDuration,
                MaxSizeInMegabytes = MaxSizeInMegabytes,
                RequiresDuplicateDetection = RequiresDuplicateDetection,
                RequiresSession = RequiresSession,
                DefaultMessageTimeToLive = DefaultMessageTimeToLive,
                EnableDeadLetteringOnMessageExpiration = EnableDeadLetteringOnMessageExpiration,
                DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow,
                MaxDeliveryCount = MaxDeliveryCount,
                EnableBatchedOperations = EnableBatchedOperations,
                EnablePartitioning = EnablePartitioning,
                SupportOrdering = SupportOrdering
            };

            try
            {
                if (config.CreateQueues())
                {
                    path = description.Path;
                    if (!Exists(namespaceClient, path))
                    {
                        namespaceClient.CreateQueue(description);
                        logger.InfoFormat("Queue '{0}' created", description.Path);
                    }
                    else
                    {
                        logger.InfoFormat("Queue '{0}' already exists, skipping creation", description.Path);
                    }
                }
                else
                {
                    logger.InfoFormat("Create queues is set to false, skipping the creation of '{0}'", description.Path);
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the queue already exists or another node beat us to it, which is ok
                logger.InfoFormat("Queue '{0}' already exists, another node probably beat us to it", description.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occured on queue creation for '{0}' going to validate if it doesn't exist", description.Path);

                // there is a chance that the timeout occurs, but the queue is created still
                // check for this
                if (!Exists(namespaceClient, path))
                {
                    throw;
                }
                else
                {
                    logger.InfoFormat("Looks like queue '{0}' exists anyway", description.Path);
                }
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient && !CreationExceptionHandling.IsCommon(ex))
                {
                    logger.Fatal(string.Format("{1} {2} occured on queue creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                    throw;
                }
                else
                {
                    logger.Info(string.Format("{1} {2} occured on queue creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                }
            }

            return description;
        }

        bool Exists(NamespaceManager namespaceClient, string path)
        {
            var key = path;
            logger.InfoFormat("Checking existence cache for '{0}'", path);
            var exists = rememberExistence.GetOrAdd(key, s =>
            {
                logger.InfoFormat("Checking namespace for existance of the queue '{0}'", path);
                return namespaceClient.QueueExists(key);
            });

            logger.InfoFormat("Determined that the queue '{0}' {1}", path, exists ? "exists" : "does not exist");

            return exists;
        }

    }
}