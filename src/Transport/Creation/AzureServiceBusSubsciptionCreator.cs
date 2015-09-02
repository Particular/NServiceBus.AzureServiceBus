namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Logging;
    using NServiceBus.Settings;

    class AzureServiceBusSubsciptionCreator : ICreateAzureServiceBusSubsciption
    {
        ReadOnlySettings settings;
        Func<string, string, ReadOnlySettings, SubscriptionDescription> subscriptionDescriptionFactory;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusSubsciptionCreator>();

        public AzureServiceBusSubsciptionCreator(ReadOnlySettings settings)
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
                    RequiresSession = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession),
                    
                    ForwardTo = setting.GetConditional<string>(subscriptionName, WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo),
                    ForwardDeadLetteredMessagesTo = setting.GetConditional<string>(subscriptionName, WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo)
                };
            }
        }

        public async Task<SubscriptionDescription> CreateAsync(string topicPath, string subsciptionName, INamespaceManager namespaceManager)
        {
            var subscriptionDescription = subscriptionDescriptionFactory(topicPath, subsciptionName, settings);

            ExceptionDispatchInfo timeoutExceptionCaught = null;

            try
            {
                if (settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                {
                    if (!await ExistsAsync(topicPath, subsciptionName, namespaceManager))
                    {
                        await namespaceManager.CreateSubscriptionAsync(subscriptionDescription);
                        logger.InfoFormat("Subscription '{0}' created", subscriptionDescription.Name);

                        var key = subscriptionDescription.TopicPath + subscriptionDescription.Name;
                        await rememberExistence.AddOrUpdate(key, keyNotFound => Task.FromResult(true), (updateTopicPath, previousValue) => Task.FromResult(true));
                    }
                    else
                    {
                        logger.InfoFormat("Subscription '{0}' already exists, skipping creation", subscriptionDescription.Name);
                        logger.InfoFormat("Checking if subscription '{0}' needs to be updated", subscriptionDescription.Name);
                        if (!(await namespaceManager.GetSubscriptionAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name)).AllMembersAreEqual(subscriptionDescription))
                        {
                            logger.InfoFormat("Updating subscription '{0}' with new description", subscriptionDescription.Name);
                            await namespaceManager.UpdateSubscriptionAsync(subscriptionDescription);
                        }
                    }
                }
                else
                {
                    logger.InfoFormat("'{0}' is set to false, skipping the creation of subscription '{0}'", WellKnownConfigurationKeys.Core.CreateTopology, subscriptionDescription.Name);
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the subscription already exists or another node beat us to it, which is ok
                logger.InfoFormat("Subscription '{0}' already exists, another node probably beat us to it", subscriptionDescription.Name);
            }
            catch (TimeoutException timeoutException)
            {
                logger.InfoFormat("Timeout occured on subscription creation for topic '{0}' subscription name '{1}' going to validate if it doesn't exist", subscriptionDescription.TopicPath, subscriptionDescription.Name);

                timeoutExceptionCaught = ExceptionDispatchInfo.Capture(timeoutException);
            }
            catch (MessagingException ex)
            {
                var loggedMessage = string.Format("{0} {1} occured on subscription '{2}' creation for topic '{3}", 
                    (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name, subscriptionDescription.Name, subscriptionDescription.TopicPath);

                if (!ex.IsTransient)
                {
                    logger.Fatal(loggedMessage, ex);
                    throw;
                }

                logger.Info(loggedMessage, ex);
            }

            if (timeoutExceptionCaught != null)
            {
                // there is a chance that the timeout occured, but the topic was still created, check again
                if (!await ExistsAsync(subscriptionDescription.TopicPath, subscriptionDescription.Name, namespaceManager, removeCacheEntry: true))
                {
                    timeoutExceptionCaught.Throw();
                }

                logger.InfoFormat("Looks like topic '{0}' exists anyway", subscriptionDescription.Name);
            }

            return subscriptionDescription;

        }

        async Task<bool> ExistsAsync(string topicPath, string subscriptionName, INamespaceManager namespaceClient, bool removeCacheEntry = false)
        {
            logger.InfoFormat("Checking existence cache for '{0}'", subscriptionName);

            var key = topicPath + subscriptionName;

            if (removeCacheEntry)
            {
                Task<bool> dummy;
                rememberExistence.TryRemove(key, out dummy);
            }

            var exists = await rememberExistence.GetOrAdd(key, async notFoundKey =>
            {
                logger.InfoFormat("Checking namespace for existance of subscription '{0}' for the topic '{1}'", subscriptionName, topicPath);
                return await namespaceClient.SubscriptionExistsAsync(topicPath, notFoundKey);
            });

            logger.InfoFormat("Determined, from cache, that the subsciption '{0}' {1}", subscriptionName, exists ? "exists" : "does not exist");

            return exists;
        }

    }
}