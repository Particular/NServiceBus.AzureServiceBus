namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;

    public static class AzureServiceBusTransportExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : ITopology, new()
        {
            var topology = Activator.CreateInstance<T>();
            return UseTopology(transportExtensions, topology);
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<T> factory) where T : ITopology
        {
            return UseTopology(transportExtensions, factory());
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, T topology) where T : ITopology
        {
            var settings = transportExtensions.GetSettings();
            settings.Set<ITopology>(topology);
            return new AzureServiceBusTopologySettings<T>(settings);
        }

        public static AzureServiceBusForwardingTopologySettings UseForwardingTopology(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set<ITopology>(new ForwardingTopology());
            return new AzureServiceBusForwardingTopologySettings(settings);
        }

        public static AzureServiceBusEndpointOrientedTopologySettings UseEndpointOrientedTopology(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set<ITopology>(new EndpointOrientedTopology());
            return new AzureServiceBusEndpointOrientedTopologySettings(settings);
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
        /// Provide custom implementation to convert brokered message to incoming message
        /// </summary>
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseBrokeredMessageToIncomingMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport>  transportExtensions) where T : IConvertBrokeredMessagesToIncomingMessages
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter, typeof(T));
        }

        /// <summary>
        /// Provide custom implementation to convert outgoing message to brokered message
        /// </summary>
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseOutgoingMessageToBrokeredMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : IConvertOutgoingMessagesToBrokeredMessages
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter, typeof(T));
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
        /// Transport type used by Azure Service Bus.
        /// <remarks>Default is TransportType.NetMessaging</remarks>
        /// </summary>
        public static void TransportType(this TransportExtensions<AzureServiceBusTransport> transportExtensions, TransportType transportType)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Connectivity.TransportType, transportType);
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
        /// Access to namespace managers configuration.
        /// </summary>
        public static AzureServiceBusNamespaceManagersSettings NamespaceManagers(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusNamespaceManagersSettings(transportExtensions.GetSettings());
        }

        /// <summary>
        /// Force usage of namespace aliases instead of raw connection strings.
        /// </summary>
        public static void UseNamespaceAliasesInsteadOfConnectionStrings(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings, true);
        }

        /// <summary>
        /// Override default namespace alias.
        /// </summary>
        public static void DefaultNamespaceAlias(this TransportExtensions<AzureServiceBusTransport> transportExtensions, string alias)
        {
            var settings = transportExtensions.GetSettings();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias, alias);
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
        /// Access to namespace routing.
        /// </summary>
        public static AzureServiceBusNamespaceRoutingSettings NamespaceRouting(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            return new AzureServiceBusNamespaceRoutingSettings(transportExtensions.GetSettings());
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