namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Features;
    using NServiceBus.Settings;

    public class BasicTopology : Feature
    {
        ITopologySectionManager topologySectionManager;
        ITransportPartsContainer container;

        public BasicTopology() : this(new TransportPartsContainer()){ }

        internal BasicTopology(ITransportPartsContainer container)
        {
            this.container = container;
            //DependsOn<UnicastBus>();
            //DependsOn<Receiving>();
            //RegisterStartupTask<SomeStartupTask>();
            Defaults(ApplyDefaults);
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            //context.Container //can only register
            //context.Pipeline //can extend
            //context.Settings //cannot change
            InitializeContainer(context.Settings);
        }

        internal void ApplyDefaults(SettingsHolder settings)
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

        internal void InitializeContainer(ReadOnlySettings settings)
        {
            // runtime components
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


        //class SomeStartupTask : FeatureStartupTask
        //{
        //    protected override Task OnStart(IBusContext context)
        //    {
        //        return Task.FromResult(true);
        //    }
        //}
    }
}