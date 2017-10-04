namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DelayedDelivery;
    using Performance.TimeToBeReceived;
    using Routing;
    using Settings;

    abstract class AzureServiceBusTransportInfrastructure : TransportInfrastructure
    {
        protected AzureServiceBusTransportInfrastructure(SettingsHolder settings)
        {
            TransactionMode = settings.SupportedTransactionMode();
            Settings = settings;
            TopologySettings = settings.GetOrCreate<TopologySettings>();

            var individualizationStrategyType = (Type)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            individualization = individualizationStrategyType.CreateInstance<IIndividualizationStrategy>(Settings);

            var compositionStrategyType = (Type)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            var compositionStrategy = compositionStrategyType.CreateInstance<ICompositionStrategy>(Settings);

            var sanitizationStrategyType = (Type)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            var sanitizationStrategy = sanitizationStrategyType.CreateInstance<ISanitizationStrategy>(Settings);

            addressingLogic = new AddressingLogic(sanitizationStrategy, compositionStrategy);
        }

        protected TopologySettings TopologySettings { get; }

        protected SettingsHolder Settings { get; }

        public override IEnumerable<Type> DeliveryConstraints => new List<Type>
        {
            typeof(DelayDeliveryWith),
            typeof(DoNotDeliverBefore),
            typeof(DiscardIfNotReceivedBefore)
        };


        public override TransportTransactionMode TransactionMode { get; }

        public override OutboundRoutingPolicy OutboundRoutingPolicy { get; } = new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);

        void InitializeIfNecessary()
        {
            if (Interlocked.Exchange(ref initializeSignaled, 1) != 0)
            {
                return;
            }
            
            defaultNamespaceAlias = Settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);
            namespaceConfigurations = Settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);

            var partitioningStrategyType = (Type)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            partitioningStrategy = partitioningStrategyType.CreateInstance<INamespacePartitioningStrategy>(Settings);

            namespaceManager = new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(Settings));
            messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(namespaceManager, Settings), Settings);

            messageReceiverLifeCycleManager = new MessageReceiverLifeCycleManager(new MessageReceiverCreator(messagingFactoryLifeCycleManager, Settings), Settings);
            senderLifeCycleManager = new MessageSenderLifeCycleManager(new MessageSenderCreator(messagingFactoryLifeCycleManager, Settings), Settings);

            oversizedMessageHandler = (IHandleOversizedBrokeredMessages)Settings.Get(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance);

            topologyManager = CreateTopologySectionManager(defaultNamespaceAlias, namespaceConfigurations, partitioningStrategy, addressingLogic);
            topologyCreator = new TopologyCreator(CreateSubscriptionCreator(), new AzureServiceBusQueueCreator(TopologySettings.QueueSettings, Settings), new AzureServiceBusTopicCreator(TopologySettings.TopicSettings), namespaceManager, Settings);
            topologyOperator = new TopologyOperator(messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(Settings, new DefaultConnectionStringToNamespaceAliasMapper(Settings)), Settings);
        }

        public override Task Start()
        {
            InitializeIfNecessary();
            return topologyManager.Initialize();
        }

        public override Task Stop()
        {
            return messagingFactoryLifeCycleManager.CloseAll();
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return new EndpointInstance(individualization.Individualize(instance.Endpoint), instance.Discriminator, instance.Properties);
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            var queue = new StringBuilder(logicalAddress.EndpointInstance.Endpoint);

            if (logicalAddress.EndpointInstance.Discriminator != null)
            {
                queue.Append($"-{logicalAddress.EndpointInstance.Discriminator}");
            }

            if (logicalAddress.Qualifier != null)
            {
                queue.Append($".{logicalAddress.Qualifier}");
            }

            return addressingLogic.Apply(queue.ToString(), EntityType.Queue).ToString();
        }

        // all dependencies required are passed in. no protected field can be assumed created
        protected abstract ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing);

        // all dependencies required are passed in. no protected field can be assumed created
        protected abstract ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator();

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                () =>
                {
                    InitializeIfNecessary();
                    return new MessagePump(topologyOperator, messageReceiverLifeCycleManager, new BrokeredMessagesToIncomingMessagesConverter(Settings, new DefaultConnectionStringToNamespaceAliasMapper(Settings)), topologyManager, Settings);
                },
                () =>
                {
                    InitializeIfNecessary();
                    return new TransportResourcesCreator(topologyCreator, topologyManager, Settings.LocalAddress());
                },
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                () =>
                {
                    InitializeIfNecessary();
                    return new Dispatcher(new OutgoingBatchRouter(new BatchedOperationsToBrokeredMessagesConverter(Settings), senderLifeCycleManager, Settings, oversizedMessageHandler), new Batcher(topologyManager, Settings));
                },
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(() =>
            {
                InitializeIfNecessary();
                return new SubscriptionManager(topologyManager, topologyOperator, topologyCreator, Settings.LocalAddress());
            });
        }

        protected IOperateTopologyInternal topologyOperator;
        // exposed as internal for component tests
        protected internal ITopologySectionManagerInternal topologyManager;
        protected IHandleOversizedBrokeredMessages oversizedMessageHandler;
        protected MessageSenderLifeCycleManager senderLifeCycleManager;
        protected MessagingFactoryLifeCycleManager messagingFactoryLifeCycleManager;
        protected MessageReceiverLifeCycleManager messageReceiverLifeCycleManager;
        protected IIndividualizationStrategy individualization;
        protected string defaultNamespaceAlias;
        protected NamespaceConfigurations namespaceConfigurations;
        protected AddressingLogic addressingLogic;
        protected NamespaceManagerLifeCycleManagerInternal namespaceManager;
        protected INamespacePartitioningStrategy partitioningStrategy;
        protected TopologyCreator topologyCreator;
        volatile int initializeSignaled;
    }
}