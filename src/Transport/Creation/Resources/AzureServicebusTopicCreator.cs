namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using Transports;

    class AzureServiceBusTopicCreator : ICreateTopics
    {
        ICreateNamespaceManagers createNamespaceManagers;
        Configure config;

        static ConcurrentDictionary<string, bool> rememberTopicExistence = new ConcurrentDictionary<string, bool>();

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

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusTopicCreator));

        public AzureServiceBusTopicCreator(ICreateNamespaceManagers createNamespaceManagers, Configure config)
        {
            this.createNamespaceManagers = createNamespaceManagers;
            this.config = config;
        }

        public TopicDescription Create(Address address)
        {
            var topicName = address.Queue;
            var namespaceClient = createNamespaceManagers.Create(address.Machine);
            var description = new TopicDescription(topicName)
            {
                // same as queue section from AzureServiceBusQueueConfig
                MaxSizeInMegabytes = MaxSizeInMegabytes,
                RequiresDuplicateDetection = RequiresDuplicateDetection,
                DefaultMessageTimeToLive = DefaultMessageTimeToLive,
                DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow,
                EnableBatchedOperations = EnableBatchedOperations,
                EnablePartitioning = EnablePartitioning,
                SupportOrdering = SupportOrdering
            };

            try
            {
                if (config.CreateQueues())
                {
                    if (!TopicExists(namespaceClient, topicName))
                    {
                        namespaceClient.CreateTopic(description);
                        logger.InfoFormat("Topic '{0}' created", description.Path);
                    }
                    else
                    {
                        logger.InfoFormat("Topic '{0}' already exists, skipping creation", description.Path);
                    }
                }
                else
                {
                    logger.InfoFormat("Create queues is set to false, skipping the creation of '{0}'", description.Path);
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the topic already exists or another node beat us to it, which is ok
                logger.InfoFormat("Topic '{0}' already exists, another node probably beat us to it", description.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occured on topic creation for '{0}' going to validate if it doesn't exist", description.Path);

                // there is a chance that the timeout occurs, but the queue is created still
                // check for this
                if (!TopicExists(namespaceClient, topicName))
                {
                    throw;
                }
                else
                {
                    logger.InfoFormat("Looks like topic '{0}' exists anyway", description.Path);
                }
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient && !CreationExceptionHandling.IsCommon(ex))
                {
                    logger.Fatal(string.Format("{1} {2} occured on topic creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                    throw;
                }
                else
                {
                    logger.Info(string.Format("{1} {2} occured on topic creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                }
            }

            return description;
        }

        bool TopicExists(NamespaceManager namespaceClient, string topicpath)
        {
            var key = topicpath;
            logger.InfoFormat("Checking cache for existence of the topic '{0}'", topicpath);
            var exists = rememberTopicExistence.GetOrAdd(key, s =>
            {
                logger.InfoFormat("Checking namespace for existence of the topic '{0}'", topicpath);
                return namespaceClient.TopicExists(key);
            });

            logger.InfoFormat("Determined that the topic '{0}' {1}", topicpath, exists ? "exists" : "does not exist");

            return exists;
        }

    }
}