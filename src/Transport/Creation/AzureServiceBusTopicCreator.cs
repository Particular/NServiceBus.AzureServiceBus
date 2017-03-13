namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using NServiceBus.AzureServiceBus;

    class AzureServiceBusTopicCreator
    {
        TopologyTopicSettings topicSettings;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusTopicCreator>();

        public AzureServiceBusTopicCreator(TopologyTopicSettings topicSettings)
        {
            this.topicSettings = topicSettings;
        }

        public async Task<TopicDescription> Create(string topicPath, INamespaceManagerInternal namespaceManager)
        {
            var topicDescription = new TopicDescription(topicPath)
            {
                SupportOrdering = topicSettings.SupportOrdering,
                MaxSizeInMegabytes = topicSettings.MaxSizeInMegabytes,
                DefaultMessageTimeToLive = topicSettings.DefaultMessageTimeToLive,
                RequiresDuplicateDetection = topicSettings.RequiresDuplicateDetection,
                DuplicateDetectionHistoryTimeWindow = topicSettings.DuplicateDetectionHistoryTimeWindow,
                EnableBatchedOperations = topicSettings.EnableBatchedOperations,
                EnablePartitioning = topicSettings.EnablePartitioning,
                AutoDeleteOnIdle = topicSettings.AutoDeleteOnIdle,
                EnableExpress = topicSettings.EnableExpress,
                EnableFilteringMessagesBeforePublishing = topicSettings.EnableFilteringMessagesBeforePublishing
            };

            topicSettings.DescriptionCustomizer(topicDescription);

            try
            {
                if (!await ExistsAsync(topicPath, namespaceManager).ConfigureAwait(false))
                {
                    await namespaceManager.CreateTopic(topicDescription).ConfigureAwait(false);
                    logger.InfoFormat("Topic '{0}' created", topicDescription.Path);
                    await rememberExistence.AddOrUpdate(topicDescription.Path, notFoundTopicPath => TaskEx.CompletedTrue, (updateTopicPath, previousValue) => TaskEx.CompletedTrue).ConfigureAwait(false);
                }
                else
                {
                    logger.InfoFormat("Topic '{0}' already exists, skipping creation", topicDescription.Path);
                    logger.InfoFormat("Checking if topic '{0}' needs to be updated", topicDescription.Path);
                    var existingTopicDescription = await namespaceManager.GetTopic(topicDescription.Path).ConfigureAwait(false);
                    if (MembersAreNotEqual(existingTopicDescription, topicDescription))
                    {
                        OverrideImmutableMembers(existingTopicDescription, topicDescription);
                        logger.InfoFormat("Updating topic '{0}' with new description", topicDescription.Path);
                        await namespaceManager.UpdateTopic(topicDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the topic already exists or another node beat us to it, which is ok
                logger.InfoFormat("Topic '{0}' already exists, another node probably beat us to it", topicDescription.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occurred on topic creation for '{0}' going to validate if it doesn't exist", topicDescription.Path);

                // there is a chance that the timeout occurred, but the topic was still created, check again
                if (!await ExistsAsync(topicDescription.Path, namespaceManager, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.InfoFormat("Looks like topic '{0}' exists anyway", topicDescription.Path);
            }
            catch (MessagingException ex)
            {
                var loggedMessage = string.Format("{1} {2} occurred on topic creation {0}", topicDescription.Path, ex.IsTransient ? "Transient" : "Non transient", ex.GetType().Name);

                if (!ex.IsTransient)
                {
                    logger.Fatal(loggedMessage, ex);
                    throw;
                }

                logger.Info(loggedMessage, ex);
            }

            return topicDescription;
        }

        async Task<bool> ExistsAsync(string topicPath, INamespaceManagerInternal namespaceClient, bool removeCacheEntry = false)
        {
            var key = topicPath + namespaceClient.Address;
            logger.InfoFormat("Checking existence cache for '{0}'", topicPath);

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, notFoundTopicPath =>
            {
                logger.InfoFormat("Checking namespace for existence of the topic '{0}'", topicPath);
                return namespaceClient.TopicExists(topicPath);
            }).ConfigureAwait(false);

            logger.InfoFormat("Determined, from cache, that the topic '{0}' {1}", topicPath, exists ? "exists" : "does not exist");

            return exists;
        }

        bool MembersAreNotEqual(TopicDescription existingDescription, TopicDescription newDescription)
        {
            if (existingDescription.RequiresDuplicateDetection != newDescription.RequiresDuplicateDetection)
            {
                logger.Warn("RequiresDuplicateDetection cannot be update on the existing queue!");
            }
            if (existingDescription.EnablePartitioning != newDescription.EnablePartitioning)
            {
                logger.Warn("EnablePartitioning cannot be update on the existing queue!");
            }

            return existingDescription.AutoDeleteOnIdle != newDescription.AutoDeleteOnIdle
                   || existingDescription.MaxSizeInMegabytes != newDescription.MaxSizeInMegabytes
                   || existingDescription.DefaultMessageTimeToLive != newDescription.DefaultMessageTimeToLive
                   || existingDescription.DuplicateDetectionHistoryTimeWindow != newDescription.DuplicateDetectionHistoryTimeWindow
                   || existingDescription.EnableBatchedOperations != newDescription.EnableBatchedOperations
                   || existingDescription.SupportOrdering != newDescription.SupportOrdering
                   || existingDescription.EnableExpress != newDescription.EnableExpress
                   || existingDescription.EnableFilteringMessagesBeforePublishing != newDescription.EnableFilteringMessagesBeforePublishing;
        }

        void OverrideImmutableMembers(TopicDescription existingDescription, TopicDescription newDescription)
        {
            newDescription.RequiresDuplicateDetection = existingDescription.RequiresDuplicateDetection;
            newDescription.EnablePartitioning = existingDescription.EnablePartitioning;
        }
    }
}