namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.AzureServiceBus.Utils;
    using Transport;

    class ForwardingTopologySectionManager : ITopologySectionManagerInternal
    {
        readonly ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();
        readonly ConcurrentDictionary<string, TopologySectionInternal> sendDestinations = new ConcurrentDictionary<string, TopologySectionInternal>();
        readonly ConcurrentDictionary<Type, TopologySectionInternal> publishDestinations = new ConcurrentDictionary<Type, TopologySectionInternal>();
        readonly List<EntityInfoInternal> topics = new List<EntityInfoInternal>();
        AsyncLazy<NamespaceBundleConfigurations> namespaceBundleConfigurations;
        string endpointName;
        INamespacePartitioningStrategy namespacePartitioningStrategy;
        AddressingLogic addressingLogic;
        string defaultNameSpaceAlias;
        NamespaceConfigurations namespaceConfigurations;
        int numberOfEntitiesInBundle;
        string bundlePrefix;

        public ForwardingTopologySectionManager(string defaultNameSpaceAlias, NamespaceConfigurations namespaceConfigurations, string endpointName, int numberOfEntitiesInBundle, string bundlePrefix, INamespacePartitioningStrategy namespacePartitioningStrategy, AddressingLogic addressingLogic, NamespaceManagerLifeCycleManagerInternal namespaceManagerLifeCycleManagerInternal)
        {
            this.bundlePrefix = bundlePrefix;
            this.numberOfEntitiesInBundle = numberOfEntitiesInBundle;
            this.namespaceConfigurations = namespaceConfigurations;
            this.defaultNameSpaceAlias = defaultNameSpaceAlias;
            this.addressingLogic = addressingLogic;
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.endpointName = endpointName;

            namespaceBundleConfigurations = new AsyncLazy<NamespaceBundleConfigurations>(async () =>
            {
                var bundleConfigurations = await NumberOfTopicsInBundleCheck.Run(namespaceManagerLifeCycleManagerInternal, namespaceConfigurations, bundlePrefix).ConfigureAwait(false);
                return bundleConfigurations;
            });
        }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public async Task<TopologySectionInternal> DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var inputQueues = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            if (!topics.Any())
            {
                await BuildTopicBundles(namespaces, addressingLogic).ConfigureAwait(false);
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

        public async Task<TopologySectionInternal> DeterminePublishDestination(Type eventType)
        {
            TopologySectionInternal destination;
            if (publishDestinations.TryGetValue(eventType, out destination))
            {
                return destination;
            }

            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();
            if (!topics.Any())
            {
                await BuildTopicBundles(namespaces, addressingLogic).ConfigureAwait(false);
            }

            var newEntry = new TopologySectionInternal
            {
                Entities = SelectFirstTopicFromBundle(topics),
                Namespaces = namespaces
            };
            publishDestinations.TryAdd(eventType, newEntry);
            return newEntry;
        }

        IEnumerable<EntityInfoInternal> SelectFirstTopicFromBundle(List<EntityInfoInternal> entityInfos)
        {
            const int index = 0;
            yield return entityInfos[index];
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            return sendDestinations.GetOrAdd(destination, d =>
            {
                var inputQueueAddress = addressingLogic.Apply(d, EntityType.Queue);

                RuntimeNamespaceInfo[] namespaces = null;
                if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultNameSpaceAlias) // sending to specific namespace
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
                        var configured = namespaceConfigurations.FirstOrDefault(n => n.Alias == inputQueueAddress.Suffix);
                        if (configured != null)
                        {
                            namespaces = new[]
                            {
                                new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose, NamespaceMode.Active)
                            };
                        }
                    }
                }
                else // sending to the partition
                {
                    namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
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

        public async Task<TopologySectionInternal> DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = await BuildSubscriptionHierarchy(eventType).ConfigureAwait(false);
            }

            return subscriptions[eventType];
        }

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventType)
        {
            TopologySectionInternal result;

            if (!subscriptions.TryRemove(eventType, out result))
            {
                result = new TopologySectionInternal
                {
                    Entities = new List<SubscriptionInfoInternal>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }

        async Task<TopologySectionInternal> BuildSubscriptionHierarchy(Type eventType)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var sanitizedInputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var sanitizedSubscriptionPath = addressingLogic.Apply(endpointName, EntityType.Subscription).Name;
            // rule name needs to be 1) based on event full name 2) unique 3) deterministic
            var ruleName = addressingLogic.Apply(eventType.FullName, EntityType.Rule).Name;

            if (!topics.Any())
            {
                await BuildTopicBundles(namespaces, addressingLogic).ConfigureAwait(false);
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
            return new TopologySectionInternal
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }

        async Task BuildTopicBundles(RuntimeNamespaceInfo[] namespaces, AddressingLogic addressingLogic)
        {
            var bundleConfigurations = await namespaceBundleConfigurations.Value.ConfigureAwait(false);

            foreach (var @namespace in namespaces)
            {
                var numberOfTopicsFound = bundleConfigurations.GetNumberOfTopicInBundle(@namespace.Alias);
                var numberOfTopicsToCreate = Math.Max(numberOfEntitiesInBundle, numberOfTopicsFound);
                for (var i = 1; i <= numberOfTopicsToCreate; i++)
                {
                    topics.AddRange(namespaces.Select(n => new EntityInfoInternal
                    {
                        Path = addressingLogic.Apply(bundlePrefix + i, EntityType.Topic).Name,
                        Type = EntityType.Topic,
                        Namespace = @namespace
                    }));
                }
            }
        }
    }
}