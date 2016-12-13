namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusSubscriptionCreator : ICreateAzureServiceBusSubscriptionsInternal
    {
        TopologySubscriptionSettings subscriptionSettings;
        int numberOfImmediateRetries;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreator>();

        public AzureServiceBusSubscriptionCreator(TopologySubscriptionSettings subscriptionSettings, ReadOnlySettings settings)
        {
            this.subscriptionSettings = subscriptionSettings;
            // TODO: remove ReadOnlySettings when the rest of setting is available
            numberOfImmediateRetries = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries);
            numberOfImmediateRetries = numberOfImmediateRetries > 0 ? numberOfImmediateRetries + 1 : subscriptionSettings.MaxDeliveryCount;
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
                ForwardDeadLetteredMessagesTo = subscriptionSettings.ForwardDeadLetteredMessagesTo,
                LockDuration = subscriptionSettings.LockDuration,
                MaxDeliveryCount = numberOfImmediateRetries
            };

            subscriptionSettings.DescriptionFactory(subscriptionDescription);

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
                    logger.Info($"Subscription '{subscriptionDescription.UserMetadata}' created as '{subscriptionDescription.Name}'");

                    var key = subscriptionDescription.TopicPath + subscriptionDescription.Name;
                    await rememberExistence.AddOrUpdate(key, keyNotFound => Task.FromResult(true), (updateTopicPath, previousValue) => Task.FromResult(true)).ConfigureAwait(false);
                }
                else
                {
                    logger.Info($"Subscription '{subscriptionDescription.Name}' aka '{subscriptionDescription.UserMetadata}' already exists, skipping creation");
                    logger.InfoFormat("Checking if subscription '{0}' needs to be updated", subscriptionDescription.Name);
                    var existingSubscriptionDescription = await namespaceManager.GetSubscription(subscriptionDescription.TopicPath, subscriptionDescription.Name).ConfigureAwait(false);
                    if (MembersAreNotEqual(existingSubscriptionDescription, subscriptionDescription))
                    {
                        logger.InfoFormat("Updating subscription '{0}' with new description", subscriptionDescription.Name);
                        await namespaceManager.UpdateSubscription(subscriptionDescription).ConfigureAwait(false);
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the subscription already exists or another node beat us to it, which is ok
                logger.InfoFormat("Subscription '{0}' already exists, another node probably beat us to it", subscriptionDescription.Name);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occurred on subscription creation for topic '{0}' subscription name '{1}' going to validate if it doesn't exist", subscriptionDescription.TopicPath, subscriptionDescription.Name);

                // there is a chance that the timeout occured, but the topic was still created, check again
                if (!await ExistsAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name, metadata.Description, namespaceManager, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.InfoFormat("Looks like subscription '{0}' exists anyway", subscriptionDescription.Name);
            }
            catch (MessagingException ex)
            {
                var loggedMessage = $"{(ex.IsTransient ? "Transient" : "Non transient")} {ex.GetType().Name} occured on subscription '{subscriptionDescription.Name}' creation for topic '{subscriptionDescription.TopicPath}'";

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
                var loggedMessage = $"{(ex.IsTransient ? "Transient" : "Non transient")} {ex.GetType().Name} occured on subscription '{subscriptionDescription.Name}' creation for topic '{subscriptionDescription.TopicPath}'";

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
            logger.Info($"Checking existence cache for subscription '{subscriptionName}' aka '{metadata}'");

            var key = topicPath + subscriptionName;

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, notFoundKey =>
            {
                logger.InfoFormat("Checking namespace for existence of subscription '{0}' for the topic '{1}'", subscriptionName, topicPath);
                return namespaceClient.SubscriptionExists(topicPath, subscriptionName);
            }).ConfigureAwait(false);

            logger.InfoFormat("Determined, from cache, that the subscription '{0}' {1}", subscriptionName, exists ? "exists" : "does not exist");

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
                   || existingDescription.ForwardDeadLetteredMessagesTo != newDescription.ForwardDeadLetteredMessagesTo;
        }
    }
}