namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusTopicSettings : ExposeSettings
    {
        TopologyTopicSettings topicSettings;

        internal AzureServiceBusTopicSettings(SettingsHolder settings) : base(settings)
        {
            topicSettings = settings.Get<ITopologyInternal>().Settings.TopicSettings;
        }

        // TODO: needs to be obsoleted with guidance
        public AzureServiceBusTopicSettings DescriptionFactory(Func<string, string, ReadOnlySettings, TopicDescription> factory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Customize topic creation by providing <see cref="TopicDescription"/>.
        /// </summary>
        public AzureServiceBusTopicSettings DescriptionFactory(Action<TopicDescription> factory)
        {
            topicSettings.DescriptionFactory = factory;

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings SupportOrdering(bool supported)
        {
            topicSettings.SupportOrdering = supported;

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            topicSettings.AutoDeleteOnIdle = autoDeleteOnIdle;
            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings DefaultMessageTimeToLive(TimeSpan timeToLive)
        {
            topicSettings.DefaultMessageTimeToLive = timeToLive;
            return this;
        }

        /// <summary>
        /// <remarks> Default is 10 minutes.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            topicSettings.DuplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            topicSettings.EnableBatchedOperations = enableBatchedOperations;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableExpress(bool enableExpress)
        {
            topicSettings.EnableExpress = enableExpress;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        // TODO: needs to be deprecated
        public AzureServiceBusTopicSettings EnableExpress(Func<string, bool> condition, bool enableExpress)
        {
            topicSettings.EnableExpress = enableExpress;
            topicSettings.EnableExpressCondition = condition;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableFilteringMessagesBeforePublishing(bool enableFilteringMessagesBeforePublishing)
        {
            topicSettings.EnableFilteringMessagesBeforePublishing = enableFilteringMessagesBeforePublishing;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// <remarks>When using <see cref="ForwardingTopology"/>, partitioning cannot be enabled.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnablePartitioning(bool enablePartitioning)
        {
            topicSettings.EnablePartitioning = enablePartitioning;
            return this;
        }

        /// <summary>
        /// <remarks> Default is 1,024 MB.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings MaxSizeInMegabytes(SizeInMegabytes maxSizeInMegabytes)
        {
            topicSettings.MaxSizeInMegabytes = maxSizeInMegabytes;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            topicSettings.RequiresDuplicateDetection = requiresDuplicateDetection;
            return this;
        }
    }
}