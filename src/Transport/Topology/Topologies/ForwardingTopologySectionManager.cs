namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using NServiceBus.Transports;
    using Settings;

    public class ForwardingTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;

        readonly ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();
        readonly List<EntityInfo> topics = new List<EntityInfo>();
        readonly Random randomGenerator = new Random();

        public ForwardingTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySection DetermineReceiveResources(string inputQueue)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(inputQueue, PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(inputQueue, EntityType.Queue);
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySection()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySection DetermineResourcesToCreate()
        {
            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }
            
            if (settings.HasExplicitValue<QueueBindings>())
            {
                var queueBindings = settings.Get<QueueBindings>();
                foreach (var n in namespaces)
                {
                    inputQueues.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                    {
                        Path = p,
                        Type = EntityType.Queue,
                        Namespace = n
                    }));

                    inputQueues.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                    {
                        Path = p,
                        Type = EntityType.Queue,
                        Namespace = n
                    }));
                }
            }

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).Where(n => n.Mode == NamespaceMode.Active).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));
            
            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }

            return new TopologySection()
            {
                Entities = SelectSingleRandomTopicFromBundle(topics),
                Namespaces = namespaces
            };
        }

        private IEnumerable<EntityInfo> SelectSingleRandomTopicFromBundle(List<EntityInfo> entityInfos)
        {
            var index = randomGenerator.Next(1, entityInfos.Count);
            yield return entityInfos[index];
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(destination, PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(destination, EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = inputQueues
            };
        }

        public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            TopologySection result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new TopologySection
                {
                    Entities = new List<SubscriptionInfo>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }

        TopologySection BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var sanitizedInputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var sanitizedSubscriptionPath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Subscription);
            // rule name needs 1) based on event full name 2) unique 3) deterministic 
            var ruleName = SHA1DeterministicNameBuilder.Build(eventType.FullName);

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }
            var subs = new List<SubscriptionInfo>();
            foreach (var topic in topics)
            {
                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = sanitizedSubscriptionPath,
                        Metadata = new ForwardingTopologySubscriptionMetadata
                        {
                            Description = $"Events {endpointName} is subscribed to",
                            SubscriptionNameBasedOnEventWithNamespace = ruleName,
                            NamespaceInfo = ns,
                            SubscribedEventFullName = eventType.FullName
                        },
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType),
                        ShouldBeListenedTo = false
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
                            Path = sanitizedInputQueuePath,
                            Type = EntityType.Queue
                        },
                        Type = EntityRelationShipType.Forward
                    });
                    return sub;
                }));
            }
            return new TopologySection()
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }
        
        void BuildTopicBundles(RuntimeNamespaceInfo[] namespaces, ISanitizationStrategy sanitizationStrategy)
        {
            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            for (var i = 1; i <= numberOfEntitiesInBundle; i++)
            {
                topics.AddRange(namespaces.Select(n => new EntityInfo
                {
                    Path = sanitizationStrategy.Sanitize(bundlePrefix + i, EntityType.Topic),
                    Type = EntityType.Topic,
                    Namespace = n
                }));
            }
        }
      
    }
}