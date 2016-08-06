namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusForwardingSubscriptionCreator : ICreateAzureServiceBusSubscriptions
    {
        ReadOnlySettings settings;
        Func<string, string, ReadOnlySettings, SubscriptionDescription> subscriptionDescriptionFactory;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreator>();

        public AzureServiceBusForwardingSubscriptionCreator(ReadOnlySettings settings)
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

        public async Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadata metadata, string sqlFilter, INamespaceManager namespaceManager, string forwardTo)
        {
            var meta = metadata as ForwardingTopologySubscriptionMetadata;
            if (meta == null)
            {
                throw new InvalidOperationException($"Cannot create subscription `{subscriptionName}` for topic `{topicPath}` without namespace inforation required.");
            }

            var subscriptionDescription = subscriptionDescriptionFactory(topicPath, subscriptionName, settings);

            subscriptionDescription.ForwardTo = forwardTo;
            subscriptionDescription.UserMetadata = metadata.Description;

            try
            {
                if (settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
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
                else
                {
                    logger.Info($"'{WellKnownConfigurationKeys.Core.CreateTopology}' is set to false, skipping the creation of subscription '{subscriptionDescription.Name}' aka '{meta.SubscribedEventFullName}'");
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

        async Task<bool> ExistsAsync(string topicPath, string subscriptionName, string metadata, INamespaceManager namespaceClient, bool removeCacheEntry = false)
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