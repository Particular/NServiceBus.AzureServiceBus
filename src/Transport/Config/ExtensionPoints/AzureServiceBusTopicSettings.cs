namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusTopicSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusTopicSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Customize topic creation by providing <see cref="TopicDescription"/>.
        /// </summary>
        public AzureServiceBusTopicSettings DescriptionFactory(Func<string, ReadOnlySettings, TopicDescription> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DescriptionFactory, factory);

            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings SupportOrdering(bool supported)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, supported);

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle, autoDeleteOnIdle);
            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings DefaultMessageTimeToLive(TimeSpan timeToLive)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive, timeToLive);
            return this;
        }

        /// <summary>
        /// <remarks> Default is 10 minutes.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, duplicateDetectionHistoryTimeWindow);
            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations, enableBatchedOperations);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableExpress(bool enableExpress)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, enableExpress);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableExpress(Func<string, bool> condition, bool enableExpress)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, enableExpress);
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpressCondition, condition);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnableFilteringMessagesBeforePublishing(bool enableFilteringMessagesBeforePublishing)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing, enableFilteringMessagesBeforePublishing);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// <remarks>When using <see cref="ForwardingTopology"/>, partitioning cannot be enabled.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings EnablePartitioning(bool enablePartitioning)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning, enablePartitioning);
            return this;
        }

        /// <summary>
        /// <remarks> Default is 1,024 MB.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings MaxSizeInMegabytes(SizeInMegabytes maxSizeInMegabytes)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes, (long)maxSizeInMegabytes);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusTopicSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection, requiresDuplicateDetection);
            return this;
        }
    }
}