namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Creation
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Logging;
    using Settings;

    class AzureServiceBusTopicCreator : ICreateAzureServiceBusTopics
    {
        ReadOnlySettings settings;
        Func<string, ReadOnlySettings, TopicDescription> topicDescriptionFactory;
        ConcurrentDictionary<string, Task<bool>> rememberExistence = new ConcurrentDictionary<string, Task<bool>>();
        ILog logger = LogManager.GetLogger<AzureServiceBusTopicCreator>();

        public AzureServiceBusTopicCreator(ReadOnlySettings settings)
        {
            this.settings = settings;

            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Resources.Topics.DescriptionFactory, out topicDescriptionFactory))
            {
                topicDescriptionFactory = (topicPath, setting) => new TopicDescription(topicPath)
                {
                    AutoDeleteOnIdle = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle),
                    DefaultMessageTimeToLive = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive),
                    DuplicateDetectionHistoryTimeWindow = setting.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow),
                    EnableBatchedOperations = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations),
                    EnableExpress = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress),
                    EnableFilteringMessagesBeforePublishing = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing),
                    EnablePartitioning = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning),
                    MaxSizeInMegabytes = setting.GetOrDefault<long>(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes),
                    RequiresDuplicateDetection = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection),
                    SupportOrdering = setting.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering),
                };
            }
        }

        public async Task<TopicDescription> CreateAsync(string topicPath, NamespaceManager namespaceManager)
        {
            var topicDescription = topicDescriptionFactory(topicPath, settings);

            try
            {
                if (settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                {
                    if (!await ExistsAsync(topicPath, namespaceManager))
                    {
                        await namespaceManager.CreateTopicAsync(topicDescription);
                        logger.InfoFormat("Topic '{0}' created", topicDescription.Path);
                        await rememberExistence.AddOrUpdate(topicDescription.Path, notFoundTopicPath => Task.FromResult(true), (updateTopicPath, previousValue) => Task.FromResult(true));
                    }
                    else
                    {
                        logger.InfoFormat("Topic '{0}' already exists, skipping creation", topicDescription.Path);
                        logger.InfoFormat("Checking if topic '{0}' needs to be updated", topicDescription.Path);
                        if (!(await namespaceManager.GetTopicAsync(topicDescription.Path)).AllMembersAreEqual(topicDescription))
                        {
                            logger.InfoFormat("Updating topic '{0}' with new description", topicDescription.Path);
                            await namespaceManager.UpdateTopicAsync(topicDescription);
                        }
                    }
                }
                else
                {
                    logger.InfoFormat("'{0}' is set to false, skipping the creation of topic '{0}'", WellKnownConfigurationKeys.Core.CreateTopology, topicDescription.Path);
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the topic already exists or another node beat us to it, which is ok
                logger.InfoFormat("Topic '{0}' already exists, another node probably beat us to it", topicDescription.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occured on topic creation for '{0}' going to validate if it doesn't exist", topicDescription.Path);

                // there is a chance that the timeout occured, but the topic was still created, check again
                if (!ExistsAsync(topicDescription.Path, namespaceManager).Result) // blocking, can't await in catch clause and can't move code out as that would otherwise break stacktrace
                {
                    throw;
                }

                logger.InfoFormat("Looks like topic '{0}' exists anyway", topicDescription.Path);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient && !CreationExceptionHandling.IsCommon(ex))
                {
                    logger.Fatal(string.Format("{1} {2} occured on topic creation {0}", topicDescription.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                    throw;
                }

                logger.Info(string.Format("{1} {2} occured on topic creation {0}", topicDescription.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
            }

            return topicDescription;
        }

        async Task<bool> ExistsAsync(string topicPath, NamespaceManager namespaceClient)
        {
            logger.InfoFormat("Checking existence cache for '{0}'", topicPath);

            var exists = await rememberExistence.GetOrAdd(topicPath, async notFoundTopicPath =>
            {
                logger.InfoFormat("Checking namespace for existance of the topic '{0}'", notFoundTopicPath);
                return await namespaceClient.TopicExistsAsync(notFoundTopicPath);
            });

            logger.InfoFormat("Determined, from cache, that the topic '{0}' {1}", topicPath, exists ? "exists" : "does not exist");

            return exists;
        }
    }
}