namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using ObjectBuilder.Common;
    using Settings;

    public class BasicTopology : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        public BasicTopology(SettingsHolder settings, IContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public void InitializeSettings()
        {
            // apply all configuration defaults
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));
        }

        public void InitializeContainer()
        {
            // runtime components
            container.Configure(typeof(NamespaceManagerCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(NamespaceManagerLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessagingFactoryCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessagingFactoryLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageReceiverCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageSenderCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(ClientEntityLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusQueueCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusTopicCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusSubscriptionCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(DefaultBrokeredMessagesToIncomingMessagesConverter), DependencyLifecycle.InstancePerCall);
            container.Configure(typeof(TopologyCreator), DependencyLifecycle.InstancePerCall);
            container.Configure(typeof(TopologyOperator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageReceiverNotifier), DependencyLifecycle.InstancePerCall);

            // strategies
            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.Configure(compositionStrategyType, DependencyLifecycle.InstancePerCall);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.Configure(individualizationStrategyType, DependencyLifecycle.InstancePerCall);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.Configure(partitioningStrategyType, DependencyLifecycle.InstancePerCall);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.Configure(sanitizationStrategyType, DependencyLifecycle.InstancePerCall);

            var validationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy);
            container.Configure(validationStrategyType, DependencyLifecycle.InstancePerCall);
        }

        public TopologyDefinition Determine(Purpose purpose)
        {
            // computes the topology

            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), purpose).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            //TODO: core has a a list of queues as well, which I suppose includes ErrorQ & AuditQ
            // integrate those correctly into the topology
            // settings.Get<QueueBindings>()

            var entities = inputQueues.ToArray();

            return new TopologyDefinition
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public IEnumerable<SubscriptionInfo> Subscribe(Type eventType)
        {
            throw new NotSupportedException("The current topology does not support azure servicebus subscriptions");
        }

        public IEnumerable<SubscriptionInfo> Unsubscribe(Type eventtype)
        {
            throw new NotSupportedException("The current topology does not support azure servicebus subscriptions");
        }
    }
}