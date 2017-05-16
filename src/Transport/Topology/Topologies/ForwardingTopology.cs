namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
        AzureServiceBusForwardingSubscriptionCreator subscriptionsCreator;
        IOperateTopologyInternal topologyOperator;
        TopologyCreator topologyCreator;
        SettingsHolder settings;
        OutgoingBatchRouter outgoingBatchRouter;
        Batcher batcher;
        IIndividualizationStrategy individualization;

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
            this.settings = settings;

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, 2);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, "bundle-");

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

            var partitioningStrategyType = (Type) settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            var partitioningStrategy = partitioningStrategyType.CreateInstance<INamespacePartitioningStrategy>(settings);

            var compositionStrategyType = (Type) settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            var compositionStrategy = compositionStrategyType.CreateInstance<ICompositionStrategy>(settings);

            var sanitizationStrategyType = (Type) settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            var sanitizationStrategy = sanitizationStrategyType.CreateInstance<ISanitizationStrategy>(settings);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            individualization = individualizationStrategyType.CreateInstance<IIndividualizationStrategy>(settings);

            var addressingLogic = new AddressingLogic(sanitizationStrategy, compositionStrategy);

            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            topologySectionManager = new ForwardingTopologySectionManager(defaultName, namespaceConfigurations, settings.EndpointName(), numberOfEntitiesInBundle, bundlePrefix, partitioningStrategy, addressingLogic);
            container.Register<ITopologySectionManagerInternal>(() => topologySectionManager);

            namespaceManagerCreator = new NamespaceManagerCreator(settings);
            namespaceManagerLifeCycleManagerInternal = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            messagingFactoryAdapterCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManagerInternal, settings);
            messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryAdapterCreator, settings);

            receiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            messageReceiverLifeCycleManager = new MessageReceiverLifeCycleManager(receiverCreator, settings);
            senderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            senderLifeCycleManager = new MessageSenderLifeCycleManager(senderCreator, settings);
            subscriptionsCreator = new AzureServiceBusForwardingSubscriptionCreator(Settings.SubscriptionSettings, settings);

            container.RegisterSingleton<DefaultConnectionStringToNamespaceAliasMapper>();

            topologyCreator = new TopologyCreator(subscriptionsCreator, queueCreator, topicCreator, namespaceManagerLifeCycleManagerInternal);

            var oversizedMessageHandler = (IHandleOversizedBrokeredMessages) settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);

            outgoingBatchRouter = new OutgoingBatchRouter(new BatchedOperationsToBrokeredMessagesConverter(settings), senderLifeCycleManager, settings, oversizedMessageHandler);
            batcher = new Batcher(topologySectionManager, settings);

            container.Register<TopologyOperator>(() => new TopologyOperator(messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings)), settings));
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
            return () => new MessagePump(topologyOperator, messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings)), topologySectionManager, settings);
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            return () => new Dispatcher(outgoingBatchRouter, batcher);
        }

        public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
        {
            return () => new SubscriptionManager(topologySectionManager, topologyOperator, topologyCreator);
        }

        public async Task<StartupCheckResult> RunPreStartupChecks()
        {
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
            return messagingFactoryLifeCycleManager.CloseAll();
        }
    }
}