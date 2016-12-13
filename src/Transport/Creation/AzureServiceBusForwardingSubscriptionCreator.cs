namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusForwardingSubscriptionCreator : ICreateAzureServiceBusSubscriptionsInternal
    {
        TopologySubscriptionSettings subscriptionSettings;
        int numberOfImmediateRetries;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreator>();

        public AzureServiceBusForwardingSubscriptionCreator(TopologySubscriptionSettings subscriptionSettings, ReadOnlySettings settings)
        {
            this.subscriptionSettings = subscriptionSettings;

            // TODO: remove ReadOnlySettings when the rest of setting is available
            numberOfImmediateRetries = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Core.RecoverabilityNumberOfImmediateRetries);
            numberOfImmediateRetries = numberOfImmediateRetries > 0 ? numberOfImmediateRetries + 1 : subscriptionSettings.MaxDeliveryCount;
        }

        public async Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo)
        {
            var meta = metadata as ForwardingTopologySubscriptionMetadata;
            if (meta == null)
            {
                throw new InvalidOperationException($"Cannot create subscription `{subscriptionName}` for topic `{topicPath}` without namespace inforation required.");
            }
            
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

            subscriptionDescription.ForwardTo = forwardTo;
            subscriptionDescription.UserMetadata = metadata.Description;

            try
            {
                if (!await ExistsAsync(topicPath, subscriptionName, metadata.Description, namespaceManager).ConfigureAwait(false))
                {
                    var ruleDescription = new RuleDescription
                    {
                        Filter = new SqlFilter(sqlFilter),
                        Name = metadata.SubscriptionNameBasedOnEventWithNamespace
                    };

                    await namespaceManager.CreateSubscription(subscriptionDescription, ruleDescription).ConfigureAwait(false);
                    logger.Info($"Subscription '{subscriptionDescription.UserMetadata}' created as '{subscriptionDescription.Name}' with rule '{ruleDescription.Name}' for event '{meta.SubscribedEventFullName}'");

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
                        logger.Info($"Updating subscription '{subscriptionDescription.Name}' with new description");
                        await namespaceManager.UpdateSubscription(subscriptionDescription).ConfigureAwait(false);
                    }

                    // Rules can't be queried, so try to add
                    var ruleDescription = new RuleDescription
                    {
                        Filter = new SqlFilter(sqlFilter),
                        Name = metadata.SubscriptionNameBasedOnEventWithNamespace
                    };
                    logger.Info($"Adding subscription rule '{ruleDescription.Name}' for event '{meta.SubscribedEventFullName}'");
                    try
                    {
                        var subscriptionClient = SubscriptionClient.CreateFromConnectionString(meta.NamespaceInfo.ConnectionString, topicPath, subscriptionName);
                        await subscriptionClient.AddRuleAsync(ruleDescription).ConfigureAwait(false);
                    }
                    catch (MessagingEntityAlreadyExistsException exception)
                    {
                        logger.Debug($"Rule '{ruleDescription.Name}' already exists. Response from the server: '{exception.Message}'");
                    }
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the subscription already exists or another node beat us to it, which is ok
                logger.Info($"Subscription '{subscriptionDescription.Name}' already exists, another node probably beat us to it");
            }
            catch (TimeoutException)
            {
                logger.Info($"Timeout occurred on subscription creation for topic '{subscriptionDescription.TopicPath}' subscription name '{subscriptionDescription.Name}' going to validate if it doesn't exist");

                // there is a chance that the timeout occured, but the topic was still created, check again
                if (!await ExistsAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name, metadata.Description, namespaceManager, removeCacheEntry: true).ConfigureAwait(false))
                {
                    throw;
                }

                logger.Info($"Looks like subscription '{subscriptionDescription.Name}' exists anyway");
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
            var meta = metadata as ForwardingTopologySubscriptionMetadata;
//            var subscriptionDescription = subscriptionDescriptionFactory(topicPath, subscriptionName, settings);
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);

            try
            {
                if (await ExistsAsync(topicPath, subscriptionName, metadata.Description, namespaceManager, removeCacheEntry: true).ConfigureAwait(false))
                {
                    var ruleDescription = new RuleDescription
                    {
                        Filter = new SqlFilter(sqlFilter),
                        Name = metadata.SubscriptionNameBasedOnEventWithNamespace
                    };
                    logger.Info($"Removing subscription rule '{ruleDescription.Name}' for event '{meta.SubscribedEventFullName}'");
                    var subscriptionClient = SubscriptionClient.CreateFromConnectionString(meta.NamespaceInfo.ConnectionString, topicPath, subscriptionName);
                    await subscriptionClient.RemoveRuleAsync(ruleDescription.Name).ConfigureAwait(false);

                    var remainingRules = await namespaceManager.GetRules(subscriptionDescription).ConfigureAwait(false);
                    if (!remainingRules.Any())
                    {
                        await namespaceManager.DeleteSubscription(subscriptionDescription).ConfigureAwait(false);
                        logger.Debug($"Subscription '{metadata.Description}' created as '{subscriptionDescription.Name}' was removed as part of unsubscribe since events are subscribed to.");
                    }
                }
            }
            catch (MessagingException ex)
            {
                var loggedMessage = $"{(ex.IsTransient ? "Transient" : "Non transient")} {ex.GetType().Name} occured on subscription '{subscriptionDescription.Name}' deletion for topic '{subscriptionDescription.TopicPath}'";

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
                logger.Info($"Checking namespace for existence of subscription '{subscriptionName}' for the topic '{topicPath}'");
                return namespaceClient.SubscriptionExists(topicPath, subscriptionName);
            }).ConfigureAwait(false);

            logger.Info($"Determined, from cache, that the subscription '{subscriptionName}' {(exists ? "exists" : "does not exist")}");

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