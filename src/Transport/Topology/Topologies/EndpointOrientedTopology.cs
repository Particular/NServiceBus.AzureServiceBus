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
            ApplyDefaults(settings);
            InitializeContainer(settings);
            queueCreator = new AzureServiceBusQueueCreator(Settings.QueueSettings, settings);
            topicCreator = new AzureServiceBusTopicCreator(Settings.TopicSettings);
        }

        void ApplyDefaults(SettingsHolder settings)
        {
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatComposition));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(CoreIndividualization));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioning));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(ThrowOnFailedValidation));
            topologySectionManager = new EndpointOrientedTopologySectionManager(settings, container);
        }

        void InitializeContainer(SettingsHolder settings)
        {
            // runtime components
            container.Register<ReadOnlySettings>(() => settings);

            container.Register<ITopologySectionManagerInternal>(() => topologySectionManager);

            namespaceManagerCreator = new NamespaceManagerCreator(settings);
            namespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            messagingFactoryAdapterCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManagerInternal, settings);
            messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryAdapterCreator, settings);
            container.Register<IManageMessagingFactoryLifeCycleInternal>(() => messagingFactoryLifeCycleManager);

            container.RegisterSingleton<MessageReceiverCreator>();
            container.RegisterSingleton<MessageReceiverLifeCycleManager>();
            container.RegisterSingleton<MessageSenderCreator>();
            container.RegisterSingleton<MessageSenderLifeCycleManager>();
            container.Register<AzureServiceBusQueueCreator>(() => queueCreator);
            container.Register<AzureServiceBusTopicCreator>(() => topicCreator);
            container.RegisterSingleton<AzureServiceBusSubscriptionCreatorV6>();

            container.RegisterSingleton<DefaultConnectionStringToNamespaceAliasMapper>();

            var brokeredMessagesToIncomingMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter);
            container.Register(brokeredMessagesToIncomingMessagesConverterType);
            var batchedOperationsToBrokeredMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter);
            container.Register(batchedOperationsToBrokeredMessagesConverterType);

            container.Register<TopologyCreator>(() => new TopologyCreator(container, namespaceManagerLifeCycleManagerInternal));
            container.Register<Batcher>();

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages)settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);
            container.Register<IHandleOversizedBrokeredMessages>(() => oversizedMessageHandler);

            container.RegisterSingleton<DefaultOutgoingBatchRouter>();
            container.RegisterSingleton<TopologyOperator>();
            container.RegisterSingleton<SubscriptionManager>();
            container.RegisterSingleton<TransportResourcesCreator>();
            container.RegisterSingleton<Dispatcher>();
            container.Register<MessagePump>();

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
            return () => container.Resolve<ICreateQueues>();
        }

        public Func<IPushMessages> GetMessagePumpFactory()
        {
            return () => container.Resolve<MessagePump>();
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            return () => container.Resolve<IDispatchMessages>();
        }

        public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
        {
            return () => container.Resolve<IManageSubscriptions>();
        }

        public Task<StartupCheckResult> RunPreStartupChecks()
        {
            var check = new ManageRightsCheck(namespaceManagerLifeCycleManagerInternal, container.Resolve<ReadOnlySettings>());

            return check.Run();
        }

        public OutboundRoutingPolicy GetOutboundRoutingPolicy()
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);
        }

        public Task Stop()
        {
            logger.Info("Closing messaging factories");
            var factories = container.Resolve<IManageMessagingFactoryLifeCycleInternal>();
            return factories.CloseAll();
        }
    }
}