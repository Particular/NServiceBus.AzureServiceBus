namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Logging;
    using Routing;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;


    class ForwardingTopologyInternal : ITopologyInternal
    {
        ILog logger = LogManager.GetLogger("ForwardingTopology");
        ITopologySectionManagerInternal topologySectionManager;
        ITransportPartsContainerInternal container;
        NamespaceManagerCreator namespaceManagerCreator;
        NamespaceManagerLifeCycleManagerInternal namespaceManagerLifeCycleManagerInternal;
        MessagingFactoryCreator messagingFactoryAdapterCreator;
        MessagingFactoryLifeCycleManager messagingFactoryLifeCycleManager;

        public ForwardingTopologyInternal() : this(new TransportPartsContainer()) { }

        internal ForwardingTopologyInternal(ITransportPartsContainerInternal container)
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
        }

        void ApplyDefaults(SettingsHolder settings)
        {
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatComposition));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(CoreIndividualization));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioning));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(ThrowOnFailedValidation));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, 2);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, "bundle-");
            topologySectionManager = new ForwardingTopologySectionManager(settings, container);
        }

        void InitializeContainer(SettingsHolder settings)
        {
            // runtime components
            container.Register<ReadOnlySettings>(() => settings);
            
            // TODO: necessary evil for now...
            container.Register<TopologyQueueSettings>(() => Settings.QueueSettings);

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
            container.RegisterSingleton<AzureServiceBusQueueCreator>();
            container.RegisterSingleton<AzureServiceBusTopicCreator>();
            container.RegisterSingleton<AzureServiceBusForwardingSubscriptionCreator>();

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

        public async Task<StartupCheckResult> RunPreStartupChecks()
        {
            var settings = container.Resolve<ReadOnlySettings>();

            var manageRightsCheck = new ManageRightsCheck(namespaceManagerLifeCycleManagerInternal, settings);

            var results = new List<StartupCheckResult>
            {
                await manageRightsCheck.Run().ConfigureAwait(false),
            };

            if (results.Any(x => x.Succeeded == false))
            {
                return StartupCheckResult.Failed(string.Join(Environment.NewLine, results.Select(x => x.ErrorMessage)));
            }

            return StartupCheckResult.Success;
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