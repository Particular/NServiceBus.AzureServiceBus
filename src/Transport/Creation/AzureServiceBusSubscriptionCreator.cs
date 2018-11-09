namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;

    class AzureServiceBusSubscriptionCreator
    {
        internal const int DefaultMaxDeliveryCountForNoImmediateRetries = int.MaxValue;

        public AzureServiceBusSubscriptionCreator(TopologySubscriptionSettings subscriptionSettings)
        {
            this.subscriptionSettings = subscriptionSettings;
        }

        public async Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo = null)
        {
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName)
            {
                EnableBatchedOperations = subscriptionSettings.EnableBatchedOperations,
                AutoDeleteOnIdle = subscriptionSettings.AutoDeleteOnIdle,
                DefaultMessageTimeToLive = subscriptionSettings.DefaultMessageTimeToLive,
                EnableDeadLetteringOnFilterEvaluationExceptions = subscriptionSettings.EnableDeadLetteringOnFilterEvaluationExceptions,
                EnableDeadLetteringOnMessageExpiration = subscriptionSettings.EnableDeadLetteringOnMessageExpiration,
                LockDuration = subscriptionSettings.LockDuration,
                MaxDeliveryCount = DefaultMaxDeliveryCountForNoImmediateRetries
            };

            if (subscriptionSettings.ForwardDeadLetteredMessagesToCondition(SubscriptionClient.FormatSubscriptionPath(topicPath, subscriptionName)))
            {
                subscriptionDescription.ForwardDeadLetteredMessagesTo = subscriptionSettings.ForwardDeadLetteredMessagesTo;
            }

            subscriptionSettings.DescriptionCustomizer(subscriptionDescription);

            if (!string.IsNullOrWhiteSpace(forwardTo))
            {
                subscriptionDescription.ForwardTo = forwardTo;
            }

            subscriptionDescription.UserMetadata = metadata.Description;

            try
            {
                if (!await ExistsAsync(topicPath, subscriptionName, metadata.Description, namespaceManager).ConfigureAwait(false))
                {
                    await namespaceManager.CreateSubscription(subscriptionDescription, sqlFilter).ConfigureAwait(false);
                    logger.Info($"Subscription '{subscriptionDescription.UserMetadata}' in namespace '{namespaceManager.Address.Host}' created as '{subscriptionDescription.Name}'.");

                    var key = GenerateSubscriptionKey(namespaceManager.Address, subscriptionDescription.TopicPath, subscriptionDescription.Name);
                    await rememberExistence.AddOrUpdate(key, keyNotFound => TaskEx.CompletedTrue, (updateTopicPath, previousValue) => TaskEx.CompletedTrue).ConfigureAwait(false);
                }
                else
                {
                    logger.Info($"Subscription '{subscriptionDescription.Name}' in namespace '{namespaceManager.Address.Host}' aka '{subscriptionDescription.UserMetadata}' already exists, skipping creation.");
                    logger.InfoFormat("Checking if subscription '{0}' in namespace '{1}' needs to be updated.", subscriptionDescription.Name, namespaceManager.Address.Host);
                    var existingSubscriptionDescription = await namespaceManager.GetSubscription(subscriptionDescription.TopicPath, subscriptionDescription.Name).ConfigureAwait(false);
                    if (MembersAreNotEqual(existingSubscriptionDescription, subscriptionDescription))
                    {
                        OverrideImmutableMembers(existingSubscriptionDescription, subscriptionDescription);
                        logger.InfoFormat("Updating subscription '{0}' in namespace '{1}' with new description.", subscriptionDescription.Name, namespaceManager.Address.Host);
                        await namespaceManager.UpdateSubscription(subscriptionDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the subscription already exists or another node beat us to it, which is ok
                logger.InfoFormat("Subscription '{0}' in namespace '{1}' already exists, another node probably beat us to it.", subscriptionDescription.Name, namespaceManager.Address.Host);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occurred on subscription creation for topic '{0}' subscription name '{1}' in namespace '{2}' going to validate if it doesn't exist.", subscriptionDescription.TopicPath, subscriptionDescription.Name, namespaceManager.Address.Host);

                // there is a chance that the timeout occurred, but the topic was still created, check again
                if (!await ExistsAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name, metadata.Description, namespaceManager, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.InfoFormat("Looks like subscription '{0}' in namespace '{1}' exists anyway.", subscriptionDescription.Name, namespaceManager.Address.Host);
            }
            catch (MessagingException ex)
            {
                var loggedMessage = $"{(ex.IsTransient ? "Transient" : "Non transient")} {ex.GetType().Name} occurred on subscription '{subscriptionDescription.Name}' creation for topic '{subscriptionDescription.TopicPath}' in namespace '{namespaceManager.Address.Host}'.";

                if (!ex.IsTransient)
                {
                    logger.Fatal(loggedMessage, ex);
                    throw;
                }

                logger.Info(loggedMessage, ex);
            }

            return subscriptionDescription;
        }

        public async Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo)
        {
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);

            try
            {
                if (await ExistsAsync(topicPath, subscriptionName, metadata.Description, namespaceManager, true).ConfigureAwait(false))
                {
                    await namespaceManager.DeleteSubscription(subscriptionDescription).ConfigureAwait(false);
                }
            }
            catch (MessagingException ex)
            {
                var loggedMessage = $"{(ex.IsTransient ? "Transient" : "Non transient")} {ex.GetType().Name} occurred on subscription '{subscriptionDescription.Name}' creation for topic '{subscriptionDescription.TopicPath}' in namespace '{namespaceManager.Address.Host}'.";

                if (!ex.IsTransient)
                {
                    logger.Fatal(loggedMessage, ex);
                    throw;
                }

                logger.Info(loggedMessage, ex);
            }
        }

        async Task<bool> ExistsAsync(string topicPath, string subscriptionName, string metadata, INamespaceManagerInternal namespaceClient, bool removeCacheEntry = false)
        {
            logger.Info($"Checking existence cache for subscription '{subscriptionName}' in namespace '{namespaceClient.Address.Host}' aka '{metadata}'.");

            var key = GenerateSubscriptionKey(namespaceClient.Address, topicPath, subscriptionName);

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, notFoundKey =>
            {
                logger.InfoFormat("Checking namespace for existence of subscription '{0}' for the topic '{1}' in namespace '{2}'.", subscriptionName, topicPath, namespaceClient.Address.Host);
                return namespaceClient.SubscriptionExists(topicPath, subscriptionName);
            }).ConfigureAwait(false);

            logger.InfoFormat("Determined, from cache, that the subscription '{0}' in namespace '{2}' {1}.", subscriptionName, exists ? "exists" : "does not exist", namespaceClient.Address.Host);

            return exists;
        }

        bool MembersAreNotEqual(SubscriptionDescription existingDescription, SubscriptionDescription newDescription)
        {
            if (existingDescription.RequiresSession != newDescription.RequiresSession)
            {
                logger.Warn("RequiresSession cannot be update on the existing queue!");
            }

            return existingDescription.AutoDeleteOnIdle != newDescription.AutoDeleteOnIdle
                   || existingDescription.LockDuration != newDescription.LockDuration
                   || existingDescription.DefaultMessageTimeToLive != newDescription.DefaultMessageTimeToLive
                   || existingDescription.EnableDeadLetteringOnMessageExpiration != newDescription.EnableDeadLetteringOnMessageExpiration
                   || existingDescription.EnableDeadLetteringOnFilterEvaluationExceptions != newDescription.EnableDeadLetteringOnFilterEvaluationExceptions
                   || existingDescription.MaxDeliveryCount != newDescription.MaxDeliveryCount
                   || existingDescription.EnableBatchedOperations != newDescription.EnableBatchedOperations
                   || ForwardToNotSame(existingDescription, newDescription)
                   || existingDescription.ForwardDeadLetteredMessagesTo != newDescription.ForwardDeadLetteredMessagesTo;
        }

        static bool ForwardToNotSame(SubscriptionDescription existingDescription, SubscriptionDescription newDescription)
        {
            var result = existingDescription.ForwardTo != newDescription.ForwardTo;
            if (!result)
            {
                return false;
            }

            if (existingDescription.ForwardTo != null)
            {
                result = !existingDescription.ForwardTo.EndsWith($"/{newDescription.ForwardTo}", StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }

        static string GenerateSubscriptionKey(Uri namespaceAddress, string topicPath, string subscriptionName)
        {
            return namespaceAddress + topicPath + subscriptionName;
        }

        void OverrideImmutableMembers(SubscriptionDescription existingDescription, SubscriptionDescription newDescription)
        {
            newDescription.RequiresSession = existingDescription.RequiresSession;
        }

        TopologySubscriptionSettings subscriptionSettings;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreator>();
    }
}