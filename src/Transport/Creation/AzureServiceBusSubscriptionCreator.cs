namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusSubscriptionCreator : ICreateAzureServiceBusSubscriptions, ICreateAzureServiceBusSubscriptionsAbleToDeleteSubscriptions
    {
        ReadOnlySettings settings;
        Func<string, string, ReadOnlySettings, SubscriptionDescription> subscriptionDescriptionFactory;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreator>();

        public AzureServiceBusSubscriptionCreator(ReadOnlySettings settings)
        {
            this.settings = settings;

            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DescriptionFactory, out subscriptionDescriptionFactory))
            {
                subscriptionDescriptionFactory = (topicPath, subscriptionName, setting) => new SubscriptionDescription(topicPath, subscriptionName)
                {
                    AutoDeleteOnIdle = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle),
                    DefaultMessageTimeToLive = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive),
                    EnableBatchedOperations = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations),
                    EnableDeadLetteringOnFilterEvaluationExceptions = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions),
                    EnableDeadLetteringOnMessageExpiration = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration),
                    LockDuration = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration),
                    MaxDeliveryCount = setting.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount),

                    ForwardDeadLetteredMessagesTo = setting.GetConditional<string>(subscriptionName, WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo)
                };
            }
        }

        public async Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo = null)
        {
            var subscriptionDescription = subscriptionDescriptionFactory(topicPath, subscriptionName, settings);

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
                    await rememberExistence.AddOrUpdate(key, keyNotFound => Task.FromResult(true), (updateTopicPath, previousValue) => Task.FromResult(true)).ConfigureAwait(false);
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

        public async Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo)
        {
            var subscriptionDescription = subscriptionDescriptionFactory(topicPath, subscriptionName, settings);

            try
            {
                if (await ExistsAsync(topicPath, subscriptionName, metadata.Description, namespaceManager, true).ConfigureAwait(false))
                {
                    var namespaceManagerAbleToDeleteSubscriptions = namespaceManager as INamespaceManagerAbleToDeleteSubscriptions;
                    if (namespaceManagerAbleToDeleteSubscriptions != null)
                    {
                        await namespaceManagerAbleToDeleteSubscriptions.DeleteSubscription(subscriptionDescription).ConfigureAwait(false);
                    }
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

        async Task<bool> ExistsAsync(string topicPath, string subscriptionName, string metadata, INamespaceManager namespaceClient, bool removeCacheEntry = false)
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
                   || existingDescription.ForwardDeadLetteredMessagesTo != newDescription.ForwardDeadLetteredMessagesTo;
        }

        static string GenerateSubscriptionKey(Uri namespaceAddress, string topicPath, string subscriptionName)
        {
            return namespaceAddress + topicPath + subscriptionName;
        }

        void OverrideImmutableMembers(SubscriptionDescription existingDescription, SubscriptionDescription newDescription)
        {
            newDescription.RequiresSession = existingDescription.RequiresSession;
        }
    }
}