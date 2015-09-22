namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using ObjectBuilder.Common;
    using Settings;

    public class BundledTopicsWithForwardingToQueuePerEndpoint : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        readonly ConcurrentDictionary<Type, List<SubscriptionInfo>> subscriptions = new ConcurrentDictionary<Type, List<SubscriptionInfo>>();
        readonly List<EntityInfo> topics = new List<EntityInfo>();

        public BundledTopicsWithForwardingToQueuePerEndpoint(SettingsHolder settings, IContainer container)
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
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, 2);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, "bundle-");
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

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologyDefinition
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        void BuildTopicBundles(NamespaceInfo[] namespaces, ISanitizationStrategy sanitizationStrategy)
        {
            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            for (var i = 0; i < numberOfEntitiesInBundle; i++)
            {
                topics.AddRange(namespaces.Select(n => new EntityInfo
                {
                    Path = sanitizationStrategy.Sanitize(bundlePrefix + i, EntityType.Topic),
                    Type = EntityType.Topic,
                    Namespace = n
                }));
            }
        }

        public IEnumerable<SubscriptionInfo> Subscribe(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        public IEnumerable<SubscriptionInfo> Unsubscribe(Type eventtype)
        {
            List<SubscriptionInfo> result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new List<SubscriptionInfo>();
            }

            return result;
        }

        List<SubscriptionInfo> BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.Get<EndpointName>();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), Purpose.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));

            var subscriptionPath = sanitizationStrategy.Sanitize(eventType.FullName, EntityType.Subscription);

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }
            var subs = new List<SubscriptionInfo>();
            foreach (var topic in topics)
            {
                subs.AddRange(namespaces.Select(ns =>
                {
                    var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);

                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = subscriptionPath,
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType)
                    };
                    sub.RelationShips.Add(new EntityRelationShipInfo
                    {
                        Source = sub,
                        Target = topic,
                        Type = EntityRelationShipType.Subscription
                    });
                    sub.RelationShips.Add(new EntityRelationShipInfo
                    {
                        Source = sub,
                        Target = new EntityInfo
                        {
                            Namespace = ns,
                            Path = inputQueuePath,
                            Type = EntityType.Queue
                        },
                        Type = EntityRelationShipType.Forward
                    });
                    return sub;
                }));
            }
            return subs;
        }
    }
}