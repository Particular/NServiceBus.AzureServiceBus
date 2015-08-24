namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    public class DefaultConfigurationValues
    {
        public SettingsHolder Apply(SettingsHolder settings)
        {
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);

            return settings;
        }

        void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
        }

        void ApplyDefaultValuesForQueueDescriptions(SettingsHolder settings)
        {
            settings.Set("Transport.CreateQueues", true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, (long) 1024);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, 6);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, null);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardToCondition, new Func<string, bool>( name => true) );
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo, null);
        }
    }
}