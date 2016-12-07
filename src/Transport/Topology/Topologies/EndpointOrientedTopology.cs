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

    
    class EndpointOrientedTopologyInternal : ITopology
    {
        ILog logger = LogManager.GetLogger("EndpointOrientedTopology");
        ITopologySectionManager topologySectionManager;
        ITransportPartsContainer container;

        public EndpointOrientedTopologyInternal() : this(new TransportPartsContainer()){ }

        internal EndpointOrientedTopologyInternal(ITransportPartsContainer container)
        {
            this.container = container;
        }

        public bool HasNativePubSubSupport => true;
        public bool HasSupportForCentralizedPubSub => true;

        public void Initialize(SettingsHolder settings)
        {
            ApplyDefaults(settings);
            InitializeContainer(settings);
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
            container.Register<ITopologySectionManager>(() => topologySectionManager);
            container.RegisterSingleton<NamespaceManagerCreator>();
            container.RegisterSingleton<NamespaceManagerLifeCycleManager>();
            container.RegisterSingleton<MessagingFactoryCreator>();
            container.RegisterSingleton<MessagingFactoryLifeCycleManager>();
            container.RegisterSingleton<MessageReceiverCreator>();
            container.RegisterSingleton<MessageReceiverLifeCycleManager>();
            container.RegisterSingleton<MessageSenderCreator>();
            container.RegisterSingleton<MessageSenderLifeCycleManager>();
            container.RegisterSingleton<AzureServiceBusQueueCreator>();
            container.RegisterSingleton<AzureServiceBusTopicCreator>();
            container.RegisterSingleton<AzureServiceBusSubscriptionCreatorV6>();

            container.RegisterSingleton<DefaultConnectionStringToNamespaceAliasMapper>();

            var brokeredMessagesToIncomingMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter);
            container.Register(brokeredMessagesToIncomingMessagesConverterType);
            var batchedOperationsToBrokeredMessagesConverterType = settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter);
            container.Register(batchedOperationsToBrokeredMessagesConverterType);

            container.Register<TopologyCreator>();
            container.Register<Batcher>();

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages)settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);
            container.Register<IHandleOversizedBrokeredMessages>(() => oversizedMessageHandler);

            container.RegisterSingleton<DefaultOutgoingBatchRouter>();
            container.RegisterSingleton<TopologyOperator>();
            container.Register<MessageReceiverNotifier>();
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
            var check = new ManageRightsCheck(container.Resolve<IManageNamespaceManagerLifeCycle>(), container.Resolve<ReadOnlySettings>());

            return check.Run();
        }

        public OutboundRoutingPolicy GetOutboundRoutingPolicy()
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);
        }

        public Task Stop()
        {
            logger.Info("Closing messaging factories");
            var factories = container.Resolve<IManageMessagingFactoryLifeCycle>();
            return factories.CloseAll();
        }
    }
}