namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusQueueCreator : ICreateAzureServiceBusQueues
    {
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ReadOnlySettings settings;
        Func<string, ReadOnlySettings, QueueDescription> descriptionFactory;
        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueCreator));
        IReadOnlyCollection<string> systemQueueAddresses;

        public AzureServiceBusQueueCreator(ReadOnlySettings settings)
        {
            this.settings = settings;
            systemQueueAddresses = settings.GetOrDefault<QueueBindings>()?.SendingAddresses ?? new List<string>();

            if(!this.settings.TryGet(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, out descriptionFactory))
            {
                descriptionFactory = (queuePath, setting) => new QueueDescription(queuePath)
                {
                    LockDuration = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration),
                    MaxSizeInMegabytes = setting.GetOrDefault<long>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes),
                    RequiresDuplicateDetection = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection),
                    DefaultMessageTimeToLive = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive),
                    EnableDeadLetteringOnMessageExpiration = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration),
                    DuplicateDetectionHistoryTimeWindow = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow),
                    MaxDeliveryCount = IsSystemQueue(queuePath) ? 10 : setting.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount),
                    EnableBatchedOperations = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations),
                    EnablePartitioning = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning),
                    SupportOrdering = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering),
                    AutoDeleteOnIdle = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle),

                    EnableExpress = setting.GetConditional<bool>(queuePath, WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress),
                    ForwardDeadLetteredMessagesTo = setting.GetConditional<string>(queuePath, WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo),
                };
            }
        }

        public async Task<QueueDescription> Create(string queuePath, INamespaceManager namespaceManager)
        {
            var description = descriptionFactory(queuePath, settings);

            try
            {
                if (!await ExistsAsync(namespaceManager, description.Path).ConfigureAwait(false))
                {
                    await namespaceManager.CreateQueue(description).ConfigureAwait(false);
                    logger.InfoFormat("Queue '{0}' created in namespace {1}", description.Path, namespaceManager.Address.Host);

                    var key = GenerateQueueKey(namespaceManager, queuePath);

                    await rememberExistence.AddOrUpdate(key, s => Task.FromResult(true), (s, b) => Task.FromResult(true)).ConfigureAwait(false);
                }
                else
                {
                    logger.InfoFormat("Queue '{0}' in namespace {1} already exists, skipping creation", description.Path, namespaceManager.Address.Host);
                    logger.InfoFormat("Checking if queue '{0}' in namespace {1} needs to be updated", description.Path, namespaceManager.Address.Host);
                    if (IsSystemQueue(description.Path))
                    {
                        logger.InfoFormat("Queue '{0}' in {1} is a shared queue and should not be updated", description.Path, namespaceManager.Address.Host);
                        return description;
                    }
                    var existingDescription = await namespaceManager.GetQueue(description.Path).ConfigureAwait(false);
                    if (MembersAreNotEqual(existingDescription, description))
                    {
                        logger.InfoFormat("Updating queue '{0}' in namespace {1} with new description", description.Path, namespaceManager.Address.Host);
                        await namespaceManager.UpdateQueue(description).ConfigureAwait(false);
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the queue already exists or another node beat us to it, which is ok
                logger.InfoFormat("Queue '{0}' in namespace {1} already exists, another node probably beat us to it", description.Path, namespaceManager.Address.Host);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occurred on queue creation for '{0}' in namespace {1} going to validate if it doesn't exist", description.Path, namespaceManager.Address.Host);

                // there is a chance that the timeout occurred, but the topic was still created, check again
                if (!await ExistsAsync(namespaceManager, description.Path, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.InfoFormat("Looks like queue '{0}' in namespace {1} exists anyway", description.Path, namespaceManager.Address.Host);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    logger.Fatal(string.Format("{1} {2} occurred on queue creation {0} in namespace {3}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name, namespaceManager.Address.Host), ex);
                    throw;
                }

                logger.Info(string.Format("{1} {2} occurred on queue creation {0} in namespace {3}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name, namespaceManager.Address.Host), ex);
            }

            return description;
        }

        bool IsSystemQueue(string queuePath)
        {
            return systemQueueAddresses.Any(address => address.Equals(queuePath, StringComparison.OrdinalIgnoreCase));
        }


        async Task<bool> ExistsAsync(INamespaceManager namespaceClient, string queuePath, bool removeCacheEntry = false)
        {
            var key = GenerateQueueKey(namespaceClient, queuePath);
            logger.InfoFormat("Checking existence cache for '{0}' in namespace {1}", queuePath, namespaceClient.Address.Host);

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, s =>
            {
                logger.InfoFormat("Checking namespace for existence of the queue '{0}' in namespace {1}", queuePath, namespaceClient.Address.Host);
                return namespaceClient.QueueExists(queuePath);
            }).ConfigureAwait(false);

            logger.InfoFormat("Determined, from cache, that the queue '{0}' in namespace {2} {1}", queuePath, exists ? "exists" : "does not exist", namespaceClient.Address.Host);

            return exists;
        }

        bool MembersAreNotEqual(QueueDescription existingDescription, QueueDescription newDescription)
        {
            if (existingDescription.RequiresDuplicateDetection != newDescription.RequiresDuplicateDetection)
            {
                logger.Warn("RequiresDuplicateDetection cannot be update on the existing queue!");
            }
            if (existingDescription.EnablePartitioning != newDescription.EnablePartitioning)
            {
                logger.Warn("EnablePartitioning cannot be update on the existing queue!");
            }
            if (existingDescription.RequiresSession != newDescription.RequiresSession)
            {
                logger.Warn("RequiresSession cannot be update on the existing queue!");
            }

            return existingDescription.AutoDeleteOnIdle != newDescription.AutoDeleteOnIdle
                   || existingDescription.LockDuration != newDescription.LockDuration
                   || existingDescription.MaxSizeInMegabytes != newDescription.MaxSizeInMegabytes
                   || existingDescription.DefaultMessageTimeToLive != newDescription.DefaultMessageTimeToLive
                   || existingDescription.EnableDeadLetteringOnMessageExpiration != newDescription.EnableDeadLetteringOnMessageExpiration
                   || existingDescription.DuplicateDetectionHistoryTimeWindow != newDescription.DuplicateDetectionHistoryTimeWindow
                   || existingDescription.MaxDeliveryCount != newDescription.MaxDeliveryCount
                   || existingDescription.EnableBatchedOperations != newDescription.EnableBatchedOperations
                   || existingDescription.SupportOrdering != newDescription.SupportOrdering
                   || existingDescription.EnableExpress != newDescription.EnableExpress
                   || existingDescription.ForwardDeadLetteredMessagesTo != newDescription.ForwardDeadLetteredMessagesTo;
        }

        static string GenerateQueueKey(INamespaceManager namespaceClient, string queuePath)
        {
            return queuePath + namespaceClient.Address;
        }
    }
}