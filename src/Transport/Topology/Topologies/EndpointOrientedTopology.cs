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
        ILog logger = LogManager.GetLogger("EndpointOrientedTopology");
        ITopologySectionManagerInternal topologySectionManager;
        ITransportPartsContainerInternal container;
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
        IOperateTopologyInternal topologyOperator;
        TopologyCreator topologyCreator;
        IConvertBrokeredMessagesToIncomingMessagesInternal brokeredMessagesToIncomingMessagesConverter;
        SettingsHolder settings;
        IConvertOutgoingMessagesToBrokeredMessagesInternal batchedOperationsToBrokeredMessagesConverter;
        DefaultOutgoingBatchRouter outgoingBatchRouter;
        Batcher batcher;
        IIndividualizationStrategy individualization;

        public EndpointOrientedTopologyInternal() : this(new TransportPartsContainer()){ }

        internal EndpointOrientedTopologyInternal(ITransportPartsContainerInternal container)
        {
            this.container = container;
        }

        public bool HasNativePubSubSupport => true;
        public bool HasSupportForCentralizedPubSub => true;
        public TopologySettings Settings { get; } = new TopologySettings();

        public void Initialize(SettingsHolder settings)
        {
            this.settings = settings;

            queueCreator = new AzureServiceBusQueueCreator(Settings.QueueSettings, settings);
            topicCreator = new AzureServiceBusTopicCreator(Settings.TopicSettings);

            InitializeContainer();
        }

        void InitializeContainer()
        {
            // runtime components
            container.Register<ReadOnlySettings>(() => settings);

            var defaultName = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);
            var namespaceConfigurations = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var conventions = settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, settings);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            var partitioningStrategy = partitioningStrategyType.CreateInstance<INamespacePartitioningStrategy>(settings);

            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            var compositionStrategy = compositionStrategyType.CreateInstance<ICompositionStrategy>(settings);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            var sanitizationStrategy = sanitizationStrategyType.CreateInstance<ISanitizationStrategy>(settings);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            individualization = individualizationStrategyType.CreateInstance<IIndividualizationStrategy>(settings);

            var addressingLogic = new AddressingLogic(sanitizationStrategy, compositionStrategy);

            topologySectionManager = new EndpointOrientedTopologySectionManager(defaultName, namespaceConfigurations, settings.EndpointName(), publishersConfiguration, partitioningStrategy, addressingLogic);
            container.Register<ITopologySectionManagerInternal>(() => topologySectionManager);

            namespaceManagerCreator = new NamespaceManagerCreator(settings);
            namespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            messagingFactoryAdapterCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManagerInternal, settings);
            messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryAdapterCreator, settings);

            receiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            messageReceiverLifeCycleManager = new MessageReceiverLifeCycleManager(receiverCreator, settings);
            senderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            senderLifeCycleManager = new MessageSenderLifeCycleManager(senderCreator, settings);
            subscriptionsCreator = new AzureServiceBusSubscriptionCreatorV6(Settings.SubscriptionSettings, settings);

            container.RegisterSingleton<DefaultConnectionStringToNamespaceAliasMapper>();

            var brokeredMessagesToIncomingMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter);
            container.Register(brokeredMessagesToIncomingMessagesConverterType);
            var batchedOperationsToBrokeredMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter);
            container.Register(batchedOperationsToBrokeredMessagesConverterType);

            brokeredMessagesToIncomingMessagesConverter = container.Resolve<IConvertBrokeredMessagesToIncomingMessagesInternal>();
            batchedOperationsToBrokeredMessagesConverter = container.Resolve<IConvertOutgoingMessagesToBrokeredMessagesInternal>();

            topologyCreator = new TopologyCreator(subscriptionsCreator, queueCreator, topicCreator, namespaceManagerLifeCycleManagerInternal);

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages)settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);
            container.Register<IHandleOversizedBrokeredMessages>(() => oversizedMessageHandler);

            outgoingBatchRouter = new DefaultOutgoingBatchRouter(batchedOperationsToBrokeredMessagesConverter, senderLifeCycleManager, settings, oversizedMessageHandler);
            batcher = new Batcher(topologySectionManager, settings);

            container.Register<TopologyOperator>(() => new TopologyOperator(messageReceiverLifeCycleManager, brokeredMessagesToIncomingMessagesConverter, settings));
            topologyOperator = container.Resolve<IOperateTopologyInternal>();
        }

        public EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return new EndpointInstance(individualization.Individualize(instance.Endpoint), instance.Discriminator, instance.Properties);
        }

        public Func<ICreateQueues> GetQueueCreatorFactory()
        {
            return () => new TransportResourcesCreator(topologyCreator, topologySectionManager);
        }

        public Func<IPushMessages> GetMessagePumpFactory()
        {
            return () => new MessagePump(topologyOperator, messageReceiverLifeCycleManager, brokeredMessagesToIncomingMessagesConverter, topologySectionManager, settings);
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            return () => new Dispatcher(outgoingBatchRouter, batcher);
        }

        public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
        {
            return () => new SubscriptionManager(topologySectionManager, topologyOperator, topologyCreator);
        }

        public Task<StartupCheckResult> RunPreStartupChecks()
        {
            var check = new ManageRightsCheck(namespaceManagerLifeCycleManagerInternal, settings);

            return check.Run();
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
    }
}