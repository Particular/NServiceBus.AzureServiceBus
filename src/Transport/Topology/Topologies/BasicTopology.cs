namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class BasicTopology : ITopology
    {
        ITopologySectionManager topologySectionManager;
        ITransportPartsContainer container;

        public BasicTopology() : this(new TransportPartsContainer()){ }

        internal BasicTopology(ITransportPartsContainer container)
        {
            this.container = container;
        }

        public void Initialize(SettingsHolder settings)
        {
            ApplyDefaults(settings);
            InitializeContainer(settings);
        }

        private void ApplyDefaults(SettingsHolder settings)
        {
            // apply all configuration defaults
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));

            topologySectionManager = new BasicTopologySectionManager(settings, container);
        }

        private void InitializeContainer(SettingsHolder settings)
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
            container.Register<DefaultBrokeredMessagesToIncomingMessagesConverter>();
            container.Register<TopologyCreator>();
            container.RegisterSingleton<TopologyOperator>();
            container.Register<MessageReceiverNotifier>();

            // strategies
            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.Register(compositionStrategyType);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.Register(individualizationStrategyType);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.Register(partitioningStrategyType);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.Register(sanitizationStrategyType);

            var validationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy);
            container.Register(validationStrategyType);
        }

        public Func<ICreateQueues> GetQueueCreatorFactory()
        {
            return () => container.Resolve<ICreateQueues>();
        }

        public Func<CriticalError, IPushMessages> GetMessagePumpFactory()
        {
            // todo, get criticial error integrated
            return error => container.Resolve<IPushMessages>();
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            return () => container.Resolve<IDispatchMessages>();
        }

        public IManageSubscriptions GetSubscriptionManager()
        {
            return null; // no clue what to return here, no implementations seem to exist in core
        }

        public OutboundRoutingPolicy GetOutboundRoutingPolicy()
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend);
        }
    }
}