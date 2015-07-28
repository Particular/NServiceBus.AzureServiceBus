namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using NServiceBus.Settings;

    class AzureServiceBusQueueCreator
    {
        static ConcurrentDictionary<string, bool> rememberExistence = new ConcurrentDictionary<string, bool>();
        ReadOnlySettings _settings;
        Func<string, ReadOnlySettings, QueueDescription> _descriptionFactory;

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueCreator));

        public AzureServiceBusQueueCreator(ReadOnlySettings settings)
        {
            _settings = settings;

            if(!_settings.TryGet(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, out _descriptionFactory))
            {
                _descriptionFactory = (name, s) => new QueueDescription(name)
                {
                    LockDuration = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration),
                    MaxSizeInMegabytes = s.GetOrDefault<long>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes),
                    RequiresDuplicateDetection = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection),
                    RequiresSession = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession),
                    DefaultMessageTimeToLive = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive),
                    EnableDeadLetteringOnMessageExpiration = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration),
                    DuplicateDetectionHistoryTimeWindow = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow),
                    MaxDeliveryCount = s.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount),
                    EnableBatchedOperations = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations),
                    EnablePartitioning = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning),
                    SupportOrdering = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering),
                    AutoDeleteOnIdle = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle),
                    EnableExpress = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress),
                    ForwardDeadLetteredMessagesTo = s.GetOrDefault<string>(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo),
                    ForwardTo = s.GetOrDefault<string>(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo),
                    IsAnonymousAccessible = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.IsAnonymousAccessible)
                };
            }
        }

        public QueueDescription Create(string entityPath, NamespaceManager namespaceManager)
        {

            var description = _descriptionFactory(entityPath, _settings);

            try
            {
                if (_settings.GetOrDefault<bool>("Transport.CreateQueues"))
                {
                    if (!Exists(namespaceManager, description.Path))
                    {
                        namespaceManager.CreateQueue(description);
                        logger.InfoFormat("Queue '{0}' created", description.Path);
                    }
                    else
                    {
                        logger.InfoFormat("Queue '{0}' already exists, skipping creation", description.Path);
                    }
                }
                else
                {
                    logger.InfoFormat("Transport.CreateQueues is set to false, skipping the creation of '{0}'", description.Path);
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
                if (!Exists(namespaceManager, description.Path))
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

            logger.InfoFormat("Determined, from cache, that the queue '{0}' {1}", path, exists ? "exists" : "does not exist");

            return exists;
        }

    }
}