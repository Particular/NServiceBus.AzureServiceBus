namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Transport;
    using Settings;

    class ForwardingTopologySectionManager : ITopologySectionManagerInternal
    {
        SettingsHolder settings;
        ITransportPartsContainerInternal container;

        readonly ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();
        readonly ConcurrentDictionary<string, TopologySectionInternal> sendDestinations = new ConcurrentDictionary<string, TopologySectionInternal>();
        readonly ConcurrentDictionary<Type, TopologySectionInternal> publishDestinations = new ConcurrentDictionary<Type, TopologySectionInternal>();
        readonly List<EntityInfoInternal> topics = new List<EntityInfoInternal>();
        readonly Random randomGenerator = new Random();

        public ForwardingTopologySectionManager(SettingsHolder settings, ITransportPartsContainerInternal container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySectionInternal()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy) container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic) container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var inputQueues = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, addressingLogic);
            }

            foreach (var n in namespaces)
            {
                inputQueues.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));

                inputQueues.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));
            }

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySectionInternal DeterminePublishDestination(Type eventType)
        {
            return publishDestinations.GetOrAdd(eventType, t =>
            {
                var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
                var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();
                var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

                if (!topics.Any())
                {
                    BuildTopicBundles(namespaces, addressingLogic);
                }

                return new TopologySectionInternal()
                {
                    Entities = SelectSingleRandomTopicFromBundle(topics),
                    Namespaces = namespaces
                };
            });
        }

        IEnumerable<EntityInfoInternal> SelectSingleRandomTopicFromBundle(List<EntityInfoInternal> entityInfos)
        {
            var index = randomGenerator.Next(0, entityInfos.Count);
            var selected = entityInfos[index];

            return entityInfos.Where(i => i.Path == selected.Path);
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            return sendDestinations.GetOrAdd(destination, d =>
            {
                var partitioningStrategy = (INamespacePartitioningStrategy) container.Resolve(typeof(INamespacePartitioningStrategy));
                var addressingLogic = (AddressingLogic) container.Resolve(typeof(AddressingLogic));
                var defaultAlias = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);

                var inputQueueAddress = addressingLogic.Apply(d, EntityType.Queue);

                RuntimeNamespaceInfo[] namespaces = null;
                if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultAlias) // sending to specific namespace
                {
                    if (inputQueueAddress.HasConnectionString)
                    {
                        namespaces = new[]
                        {
                            new RuntimeNamespaceInfo(inputQueueAddress.Suffix, inputQueueAddress.Suffix, NamespacePurpose.Routing, NamespaceMode.Active)
                        };
                    }
                    else
                    {
                        NamespaceConfigurations configuredNamespaces;
                        if (settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out configuredNamespaces))
                        {
                            var configured = configuredNamespaces.FirstOrDefault(n => n.Alias == inputQueueAddress.Suffix);
                            if (configured != null)
                            {
                                namespaces = new[]
                                {
                                    new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose, NamespaceMode.Active)
                                };
                            }
                        }
                    }
                }
                else // sending to the partition
                {
                    namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
                }

                if (namespaces == null)
                {
                    throw new Exception($"Could not determine namespace for destination `{d}`.");
                }
                var inputQueues = namespaces.Select(n => new EntityInfoInternal
                {
                    Path = inputQueueAddress.Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }).ToArray();

                return new TopologySectionInternal
                {
                    Namespaces = namespaces,
                    Entities = inputQueues
                };
            });

        }

        public TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            TopologySectionInternal result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new TopologySectionInternal
                {
                    Entities = new List<SubscriptionInfoInternal>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }

        TopologySectionInternal BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var sanitizedInputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var sanitizedSubscriptionPath = addressingLogic.Apply(endpointName, EntityType.Subscription).Name;
            // rule name needs to be 1) based on event full name 2) unique 3) deterministic
            var ruleName = addressingLogic.Apply(eventType.FullName, EntityType.Rule).Name;

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, addressingLogic);
            }
            var subs = new List<SubscriptionInfoInternal>();
            foreach (var topic in topics)
            {
                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfoInternal
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
                    sub.RelationShips.Add(new EntityRelationShipInfoInternal
                    {
                        Source = sub,
                        Target = topic,
                        Type = EntityRelationShipTypeInternal.Subscription
                    });
                    sub.RelationShips.Add(new EntityRelationShipInfoInternal
                    {
                        Source = sub,
                        Target = new EntityInfoInternal
                        {
                            Namespace = ns,
                            Path = sanitizedInputQueuePath,
                            Type = EntityType.Queue
                        },
                        Type = EntityRelationShipTypeInternal.Forward
                    });
                    return sub;
                }));
            }
            return new TopologySectionInternal()
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }

        void BuildTopicBundles(RuntimeNamespaceInfo[] namespaces, AddressingLogic addressingLogic)
        {
            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            for (var i = 1; i <= numberOfEntitiesInBundle; i++)
            {
                topics.AddRange(namespaces.Select(n => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(bundlePrefix + i, EntityType.Topic).Name,
                    Type = EntityType.Topic,
                    Namespace = n
                }));
            }
        }

    }
}