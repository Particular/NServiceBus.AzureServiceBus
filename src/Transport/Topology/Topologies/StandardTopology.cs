namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Features;
    using NServiceBus.Settings;

    public class StandardTopology : Feature
    {
        ITopologySectionManager topologySectionManager;
        ITransportPartsContainer container;

        internal StandardTopology()
        {
            container = new TransportPartsContainer();
            //DependsOn<UnicastBus>();
            //DependsOn<Receiving>();
            Defaults(ApplyDefaults);
        }

        internal StandardTopology(ITransportPartsContainer container)
        {
            this.container = container;
            //DependsOn<UnicastBus>();
            //DependsOn<Receiving>();
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
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));

            topologySectionManager = new StandardTopologySectionManager(settings, container);
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
            container.RegisterSingleton<AzureServiceBusTopicCreator>();
            container.RegisterSingleton<AzureServiceBusSubscriptionCreator>();
            container.Register<DefaultBrokeredMessagesToIncomingMessagesConverter>();
            container.Register<TopologyCreator>();
            container.RegisterSingleton<TopologyOperator>();
            container.Register<MessageReceiverNotifier>();

            // configures container
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

    }
}