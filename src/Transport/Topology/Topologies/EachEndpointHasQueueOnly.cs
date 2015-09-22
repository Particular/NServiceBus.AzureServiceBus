namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using ObjectBuilder.Common;
    using Settings;

    public class EachEndpointHasQueueOnly : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        public EachEndpointHasQueueOnly(SettingsHolder settings, IContainer container)
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
            // configures container
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