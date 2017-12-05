namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using Settings;

    class AzureServiceBusQueueCreator
    {
        internal const int DefaultMaxDeliveryCountForNoImmediateRetries = 1;

        public AzureServiceBusQueueCreator(TopologyQueueSettings queueSettings, ReadOnlySettings settings)
        {
            this.queueSettings = queueSettings;
            // TODO: remove ReadOnlySettings when the rest of setting is available
            systemQueueAddresses = settings.GetOrDefault<QueueBindings>()?.SendingAddresses ?? new List<string>();
            numberOfImmediateRetries = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries);
            // If immediate retries are disabled (0), use 1. Otherwise, immediate retries + 1
            numberOfImmediateRetries = Math.Max(DefaultMaxDeliveryCountForNoImmediateRetries, numberOfImmediateRetries + 1);
        }

        public async Task<QueueDescription> Create(string queuePath, INamespaceManagerInternal namespaceManager)
        {
            var description = new QueueDescription(queuePath)
            {
                LockDuration = queueSettings.LockDuration,
                MaxSizeInMegabytes = queueSettings.MaxSizeInMegabytes,
                RequiresDuplicateDetection = queueSettings.RequiresDuplicateDetection,
                DefaultMessageTimeToLive = queueSettings.DefaultMessageTimeToLive,
                EnableDeadLetteringOnMessageExpiration = queueSettings.EnableDeadLetteringOnMessageExpiration,
                DuplicateDetectionHistoryTimeWindow = queueSettings.DuplicateDetectionHistoryTimeWindow,
                MaxDeliveryCount = IsSystemQueue(queuePath) ? 10 : numberOfImmediateRetries,
                EnableBatchedOperations = queueSettings.EnableBatchedOperations,
                EnablePartitioning = queueSettings.EnablePartitioning,
                SupportOrdering = queueSettings.SupportOrdering,
                AutoDeleteOnIdle = queueSettings.AutoDeleteOnIdle,
                ForwardDeadLetteredMessagesTo = queueSettings.ForwardDeadLetteredMessagesTo
            };

            queueSettings.DescriptionCustomizer(description);

            try
            {
                if (!await ExistsAsync(namespaceManager, description.Path).ConfigureAwait(false))
                {
                    await namespaceManager.CreateQueue(description).ConfigureAwait(false);
                    logger.InfoFormat("Queue '{0}' created", description.Path);

                    await rememberExistence.AddOrUpdate(description.Path, s => TaskEx.CompletedTrue, (s, b) => TaskEx.CompletedTrue).ConfigureAwait(false);
                }
                else
                {
                    logger.InfoFormat("Queue '{0}' already exists, skipping creation", description.Path);
                    logger.InfoFormat("Checking if queue '{0}' needs to be updated", description.Path);
                    if (IsSystemQueue(description.Path))
                    {
                        logger.InfoFormat("Queue '{0}' is a shared queue and should not be updated", description.Path);
                        return description;
                    }
                    var existingDescription = await namespaceManager.GetQueue(description.Path).ConfigureAwait(false);
                    if (MembersAreNotEqual(existingDescription, description))
                    {
                        OverrideImmutableMembers(existingDescription, description);
                        logger.InfoFormat("Updating queue '{0}' with new description", description.Path);
                        await namespaceManager.UpdateQueue(description).ConfigureAwait(false);
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the queue already exists or another node beat us to it, which is ok
                logger.InfoFormat("Queue '{0}' already exists, another node probably beat us to it", description.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occurred on queue creation for '{0}' going to validate if it doesn't exist", description.Path);

                // there is a chance that the timeout occurred, but the topic was still created, check again
                if (!await ExistsAsync(namespaceManager, description.Path, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.InfoFormat("Looks like queue '{0}' exists anyway", description.Path);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    logger.Fatal(string.Format("{1} {2} occurred on queue creation {0}", description.Path, ex.IsTransient ? "Transient" : "Non transient", ex.GetType().Name), ex);
                    throw;
                }

                logger.Info(string.Format("{1} {2} occurred on queue creation {0}", description.Path, ex.IsTransient ? "Transient" : "Non transient", ex.GetType().Name), ex);
            }

            return description;
        }

        bool IsSystemQueue(string queuePath)
        {
            return systemQueueAddresses.Any(address => address.Equals(queuePath, StringComparison.OrdinalIgnoreCase));
        }

        async Task<bool> ExistsAsync(INamespaceManagerInternal namespaceClient, string queuePath, bool removeCacheEntry = false)
        {
            var key = queuePath + namespaceClient.Address;
            logger.InfoFormat("Checking existence cache for '{0}'", queuePath);

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, s =>
            {
                logger.InfoFormat("Checking namespace for existence of the queue '{0}'", queuePath);
                return namespaceClient.QueueExists(queuePath);
            }).ConfigureAwait(false);

            logger.InfoFormat("Determined, from cache, that the queue '{0}' {1}", queuePath, exists ? "exists" : "does not exist");

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

        void OverrideImmutableMembers(QueueDescription existingDescription, QueueDescription newDescription)
        {
            newDescription.RequiresDuplicateDetection = existingDescription.RequiresDuplicateDetection;
            newDescription.EnablePartitioning = existingDescription.EnablePartitioning;
            newDescription.RequiresSession = existingDescription.RequiresSession;
        }

        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        TopologyQueueSettings queueSettings;
        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueCreator));
        IReadOnlyCollection<string> systemQueueAddresses;
        int numberOfImmediateRetries;
    }
}