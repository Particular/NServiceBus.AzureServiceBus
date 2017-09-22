namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Routing;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

    class EndpointOrientedTopologyInternal : ITopologyInternal
    {
        public ITopologySectionManagerInternal TopologySectionManager { get; set; }

        public IOperateTopologyInternal Operator { get; set; }

        public bool HasNativePubSubSupport => true;
        public bool HasSupportForCentralizedPubSub => true;
        public TopologySettings Settings { get; } = new TopologySettings();

        public void Initialize(SettingsHolder settings)
        {
            this.settings = settings;

            queueCreator = new AzureServiceBusQueueCreator(Settings.QueueSettings, settings);
            topicCreator = new AzureServiceBusTopicCreator(Settings.TopicSettings);

            var defaultName = this.settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);
            var namespaceConfigurations = this.settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var conventions = this.settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, this.settings);

            var partitioningStrategyType = (Type)this.settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            var partitioningStrategy = partitioningStrategyType.CreateInstance<INamespacePartitioningStrategy>(this.settings);

            var compositionStrategyType = (Type)this.settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            var compositionStrategy = compositionStrategyType.CreateInstance<ICompositionStrategy>(this.settings);

            var sanitizationStrategyType = (Type)this.settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            var sanitizationStrategy = sanitizationStrategyType.CreateInstance<ISanitizationStrategy>(this.settings);

            var individualizationStrategyType = (Type)this.settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            individualization = individualizationStrategyType.CreateInstance<IIndividualizationStrategy>(this.settings);

            var addressingLogic = new AddressingLogic(sanitizationStrategy, compositionStrategy);

            var endpointName = this.settings.EndpointName();
            TopologySectionManager = new EndpointOrientedTopologySectionManager(defaultName, namespaceConfigurations, endpointName, publishersConfiguration, partitioningStrategy, addressingLogic);

            namespaceManagerCreator = new NamespaceManagerCreator(this.settings);
            namespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            messagingFactoryAdapterCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManagerInternal, this.settings);
            messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryAdapterCreator, this.settings);

            receiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, this.settings);
            messageReceiverLifeCycleManager = new MessageReceiverLifeCycleManager(receiverCreator, this.settings);
            senderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, this.settings);
            senderLifeCycleManager = new MessageSenderLifeCycleManager(senderCreator, this.settings);
            subscriptionsCreator = new AzureServiceBusSubscriptionCreatorV6(Settings.SubscriptionSettings, this.settings);

            topologyCreator = new TopologyCreator(subscriptionsCreator, queueCreator, topicCreator, namespaceManagerLifeCycleManagerInternal, this.settings);

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages)this.settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);

            outgoingBatchRouter = new OutgoingBatchRouter(new BatchedOperationsToBrokeredMessagesConverter(this.settings), senderLifeCycleManager, this.settings, oversizedMessageHandler);
            batcher = new Batcher(TopologySectionManager, this.settings);

            Operator = new TopologyOperator(messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(this.settings, new DefaultConnectionStringToNamespaceAliasMapper(this.settings)), this.settings);
        }

        public EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return new EndpointInstance(individualization.Individualize(instance.Endpoint), instance.Discriminator, instance.Properties);
        }

        public Func<ICreateQueues> GetQueueCreatorFactory()
        {
            return () => new TransportResourcesCreator(topologyCreator, TopologySectionManager);
        }

        public Func<IPushMessages> GetMessagePumpFactory()
        {
            return () => new MessagePump(Operator, messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings)), TopologySectionManager, settings);
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            return () => new Dispatcher(outgoingBatchRouter, batcher);
        }

        public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
        {
            // Have to provide endpoint name by accessing the settings and not using the cached version for an endpoint name that is overridden.
            return () => new SubscriptionManager(TopologySectionManager, Operator, topologyCreator, settings.LocalAddress());
        }

        public Task<StartupCheckResult> RunPreStartupChecks()
        {
            return Task.FromResult(StartupCheckResult.Success);
        }

        public OutboundRoutingPolicy GetOutboundRoutingPolicy()
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);
        }

        public Task Stop()
        {
            logger.Info("Closing messaging factories");
            return messagingFactoryLifeCycleManager.CloseAll();
        }

        ILog logger = LogManager.GetLogger("EndpointOrientedTopology");
        AzureServiceBusQueueCreator queueCreator;
        AzureServiceBusTopicCreator topicCreator;
        NamespaceManagerCreator namespaceManagerCreator;
        NamespaceManagerLifeCycleManagerInternal namespaceManagerLifeCycleManagerInternal;
        MessagingFactoryCreator messagingFactoryAdapterCreator;
        MessagingFactoryLifeCycleManager messagingFactoryLifeCycleManager;
        MessageReceiverCreator receiverCreator;
        MessageReceiverLifeCycleManager messageReceiverLifeCycleManager;
        MessageSenderCreator senderCreator;
        MessageSenderLifeCycleManager senderLifeCycleManager;
        AzureServiceBusSubscriptionCreatorV6 subscriptionsCreator;
        TopologyCreator topologyCreator;
        SettingsHolder settings;
        OutgoingBatchRouter outgoingBatchRouter;
        Batcher batcher;
        IIndividualizationStrategy individualization;
    }
}