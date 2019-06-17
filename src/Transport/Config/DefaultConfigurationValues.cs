namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using System;
    using Topology.MetaModel;
    using Transport.AzureServiceBus;

    static class DefaultConfigurationValues
    {
        public static SettingsHolder Apply(SettingsHolder settings)
        {
            settings.SetDefault<TopologySettings>(new TopologySettings());

            ApplyDefaultsForExtensibility(settings);
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForAddressing(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);
            ApplyDefaultValuesForTopics(settings);
            ApplyDefaultValuesForSubscriptions(settings);
            ApplyDefaultValuesForRules(settings);
            ApplyDefaultValuesForSerialization(settings);

            return settings;
        }

        static void ApplyDefaultsForExtensibility(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatComposition));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(CoreIndividualization));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioning));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(ThrowOnFailedValidation));
        }

        static void ApplyDefaultValuesForSerialization(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.ByteArray);
        }

        static void ApplyDefaultValuesForAddressing(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, new NamespaceConfigurations());
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias, "default");
        }

        static void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            var numberOfLogicalCores = Math.Max(2, Environment.ProcessorCount);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, numberOfLogicalCores);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, true);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.ConnectivityMode, ConnectivityMode.Tcp);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.TransportType, TransportType.NetMessaging); // can't make TransportType.Amqp the default due to performance issues investigated with ASB team
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, numberOfLogicalCores);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, TimeSpan.FromMinutes(5.0));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 20);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval, TimeSpan.FromMilliseconds(20));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, TimeSpan.FromSeconds(10));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes, 256);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance, new ThrowOnOversizedBrokeredMessages());
        }

        static void ApplyDefaultValuesForQueueDescriptions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, 260);
        }

        static void ApplyDefaultValuesForTopics(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, 260);
        }

        static void ApplyDefaultValuesForSubscriptions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, 50);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.BrokerSideSubscriptionFilterFactoryInstance, new DefaultCreateBrokerSideSubscriptionFilter());
        }

        static void ApplyDefaultValuesForRules(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, 50);
        }
    }
}