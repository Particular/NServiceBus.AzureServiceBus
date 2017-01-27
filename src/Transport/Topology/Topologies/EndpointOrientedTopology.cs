namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using AzureServiceBus;
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

            ApplyDefaults();

            queueCreator = new AzureServiceBusQueueCreator(Settings.QueueSettings, settings);
            topicCreator = new AzureServiceBusTopicCreator(Settings.TopicSettings);

            InitializeContainer();
        }

        void ApplyDefaults()
        {
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatComposition));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(CoreIndividualization));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioning));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(ThrowOnFailedValidation));
            topologySectionManager = new EndpointOrientedTopologySectionManager(settings, container);
        }

        void InitializeContainer()
        {
            // runtime components
            container.Register<ReadOnlySettings>(() => settings);

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
            container.Register<TopologyCreator>(() => topologyCreator);

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages)settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);
            container.Register<IHandleOversizedBrokeredMessages>(() => oversizedMessageHandler);

            outgoingBatchRouter = new DefaultOutgoingBatchRouter(batchedOperationsToBrokeredMessagesConverter, senderLifeCycleManager, settings, oversizedMessageHandler);
            batcher = new Batcher(topologySectionManager, settings);

            container.Register<TopologyOperator>(() => new TopologyOperator(messageReceiverLifeCycleManager, brokeredMessagesToIncomingMessagesConverter, settings));
            topologyOperator = container.Resolve<IOperateTopologyInternal>();

            container.Register<AddressingLogic>();

            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.Register(compositionStrategyType);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.Register(individualizationStrategyType);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.Register(partitioningStrategyType);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.Register(sanitizationStrategyType);

            var conventions = settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, settings);
            container.Register<PublishersConfiguration>(() => publishersConfiguration);
        }

        public EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            var individualization = container.Resolve<IIndividualizationStrategy>();
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