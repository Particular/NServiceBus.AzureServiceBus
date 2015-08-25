namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusTopicSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusTopicSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusTopicSettings DescriptionFactory(Func<string, ReadOnlySettings, TopicDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DescriptionFactory, factory);

            return this;
        }

        public AzureServiceBusTopicSettings SupportOrdering(bool supported)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, supported);

            return this;
        }

        public AzureServiceBusTopicSettings AutoDeleteOnIdle(bool autoDeleteOnIdle)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle, autoDeleteOnIdle);
            return this;
        }

        public AzureServiceBusTopicSettings DefaultMessageTimeToLive(TimeSpan timeToLive)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive, timeToLive);
            return this;
        }

        public AzureServiceBusTopicSettings DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, duplicateDetectionHistoryTimeWindow);
            return this;
        }

        public AzureServiceBusTopicSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations, enableBatchedOperations);
            return this;
        }

        public AzureServiceBusTopicSettings EnableExpress(bool enableExpress)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, enableExpress);
            return this;
        }

        public AzureServiceBusTopicSettings EnableFilteringMessagesBeforePublishing(bool enableFilteringMessagesBeforePublishing)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing, enableFilteringMessagesBeforePublishing);
            return this;
        }

        public AzureServiceBusTopicSettings EnablePartitioning(bool enablePartitioning)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning, enablePartitioning);
            return this;
        }

        public AzureServiceBusTopicSettings MaxSizeInMegabytes(long maxSizeInMegabytes)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes, maxSizeInMegabytes);
            return this;
        }

        public AzureServiceBusTopicSettings RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection, requiresDuplicateDetection);
            return this;
        }
    }
}