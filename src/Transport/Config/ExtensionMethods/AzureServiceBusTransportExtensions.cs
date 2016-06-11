namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using AzureServiceBus;
    using AzureServiceBus.Topology.MetaModel;
    using Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTransportExtensions
    {
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : ITopology, new()
        {
            var topology = Activator.CreateInstance<T>();
            return UseTopology(transportExtensions, topology);
        }

        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<T> factory) where T : ITopology
        {
            return UseTopology(transportExtensions, factory());
        }

        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, T topology) where T : ITopology
        {
            var settings = transportExtensions.GetSettings();
            settings.Set<ITopology>(topology);
            return new AzureServiceBusTopologySettings<T>(settings);
        }


        /// <summary>
        /// <see cref="BrokeredMessage"/> body type used to store and retrieve messages.
        /// <remarks>Default is SupportedBrokeredMessageBodyTypes.ByteArray.</remarks>
        /// </summary>
        public static void BrokeredMessageBodyType(this TransportExtensions<AzureServiceBusTransport> transportExtensions, SupportedBrokeredMessageBodyTypes type)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, type);
        }

        /// <summary>
        /// Number of senders and receivers per entity.
        /// <remarks>Default is 5.</remarks>
        /// </summary>
        public static void NumberOfClientsPerEntity(this TransportExtensions<AzureServiceBusTransport> transportExtensions, int number)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, number);
        }

        /// <summary>
        /// Use receive queue to dispatch outgoing messages.
        /// <remarks>Default is true.</remarks>
        /// </summary>
        public static void SendViaReceiveQueue(this TransportExtensions<AzureServiceBusTransport> transportExtensions, bool sendViaReceiveQueue)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, sendViaReceiveQueue);
        }

        /// <summary>
        /// Connectivity mode used by Azure Service Bus.
        /// <remarks>Default is ConnectivityMode.Tcp</remarks>
        /// </summary>
        public static void ConnectivityMode(this TransportExtensions<AzureServiceBusTransport> transportExtensions, ConnectivityMode connectivityMode)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Connectivity.ConnectivityMode, connectivityMode);
        }


        /// <summary>
        /// Access to message receivers configuration.
        /// </summary>
        public static AzureServiceBusMessageReceiverSettings MessageReceivers(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusMessageReceiverSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to message senders configuration.
        /// </summary>
        public static AzureServiceBusMessageSenderSettings MessageSenders(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusMessageSenderSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to messaging factories configuration.
        /// </summary>
        public static AzureServiceBusMessagingFactoriesSettings MessagingFactories(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusMessagingFactoriesSettings(transportExtensions.GetSettings());
        }


        /// <summary>
        /// Force usage of namespace names instead of raw connection strings.
        /// </summary>
        public static void UseNamespaceNamesInsteadOfConnectionStrings(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings, typeof(PassThroughNamespaceNameToConnectionStringMapper));
        }

        /// <summary>
        /// Access to queues configuration.
        /// </summary>
        public static AzureServiceBusQueueSettings Queues(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusQueueSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to topics configuration.
        /// </summary>
        public static AzureServiceBusTopicSettings Topics(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusTopicSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to subscriptions configuration.
        /// </summary>
        public static AzureServiceBusSubscriptionSettings Subscriptions(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusSubscriptionSettings(transportExtensions.GetSettings());
        }


        /// <summary>
        /// Access to namespace partitioning configuration.
        /// </summary>
        public static AzureServiceBusNamespacePartitioningSettings NamespacePartitioning(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusNamespacePartitioningSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to entities composition configuration.
        /// </summary>
        public static AzureServiceBusCompositionSettings Composition(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusCompositionSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Access to entities path/name sanitization configuration.
        /// </summary>
        public static AzureServiceBusSanitizationSettings Sanitization(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusSanitizationSettings(transportExtensions.GetSettings());
        }


        /// <summary>
        /// Access to input queue individualization configuration.
        /// </summary>
        public static AzureServiceBusIndividualizationSettings Individualization(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusIndividualizationSettings(transportExtensions.GetSettings());
        }
    }
}