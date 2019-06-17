namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.Connectivity;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;

    class ForwardingTopologySectionManager : ITopologySectionManagerInternal
    {
        public ForwardingTopologySectionManager(string defaultNameSpaceAlias, NamespaceConfigurations namespaceConfigurations, string originalEndpointName, int numberOfEntitiesInBundle, string bundlePrefix, INamespacePartitioningStrategy namespacePartitioningStrategy, AddressingLogic addressingLogic, ICreateBrokerSideSubscriptionFilter brokerSideSubscriptionFilterFactory)
        {
            this.bundlePrefix = bundlePrefix;
            this.numberOfEntitiesInBundle = numberOfEntitiesInBundle;
            this.namespaceConfigurations = namespaceConfigurations;
            this.defaultNameSpaceAlias = defaultNameSpaceAlias;
            this.addressingLogic = addressingLogic;
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.originalEndpointName = originalEndpointName;
            this.brokerSideSubscriptionFilterFactory = brokerSideSubscriptionFilterFactory;
        }

        public NamespaceBundleConfigurations BundleConfigurations { get; set; }

        public Func<Task> Initialize { get; set; }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();
            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;

            var entities = new List<EntityInfoInternal>();
            foreach (var n in namespaces)
            {
                entities.Add(new EntityInfoInternal
                {
                    Path = inputQueuePath,
                    Type = EntityType.Queue,
                    Namespace = n
                });
            }

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySectionInternal DetermineTopicsToCreate(string localAddress)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = CreateTopics(namespaces)
            };
        }

        public TopologySectionInternal DetermineQueuesToCreate(QueueBindings queueBindings, string localAddress)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();
            var inputQueuePath = addressingLogic.Apply(localAddress, EntityType.Queue).Name;
            var entities = new List<EntityInfoInternal>();

            foreach (var n in namespaces)
            {
                entities.Add(new EntityInfoInternal
                {
                    Path = inputQueuePath,
                    Type = EntityType.Queue,
                    Namespace = n
                });

                entities.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));

                entities.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));
            }

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySectionInternal DeterminePublishDestination(Type eventType, string localAddress)
        {
            return namespacePartitioningStrategy.SendingNamespacesCanBeCached ? publishDestinations.GetOrAdd(eventType, t => CreateSectionForPublish()) : CreateSectionForPublish();
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            return namespacePartitioningStrategy.SendingNamespacesCanBeCached ? sendDestinations.GetOrAdd(destination, d => CreateSectionForSend(d)) : CreateSectionForSend(destination);
        }

        public TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType, string localAddress)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType, localAddress);
            }

            return subscriptions[eventType];
        }

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventType)
        {
            if (!subscriptions.TryRemove(eventType, out var result))
            {
                result = new TopologySectionInternal
                {
                    Entities = new List<SubscriptionInfoInternal>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }

        List<EntityInfoInternal> CreateTopics(RuntimeNamespaceInfo[] namespaces)
        {
            var entities = new List<EntityInfoInternal>();
            foreach (var @namespace in namespaces)
            {
                var numberOfTopicsFound = BundleConfigurations.GetNumberOfTopicInBundle(@namespace.Alias);
                var numberOfTopicsToCreate = Math.Max(numberOfEntitiesInBundle, numberOfTopicsFound);
                for (var i = 1; i <= numberOfTopicsToCreate; i++)
                {
                    entities.AddRange(namespaces.Select(n => new EntityInfoInternal
                    {
                        Path = addressingLogic.Apply($"{bundlePrefix}{i}", EntityType.Topic).Name,
                        Type = EntityType.Topic,
                        Namespace = @namespace
                    }));
                }
            }
            return entities;
        }

        TopologySectionInternal CreateSectionForPublish()
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();

            var entityInfoInternals = CreateTopics(namespaces);

            var passiveTopics = entityInfoInternals.Where(e => e.Namespace.Mode == NamespaceMode.Passive);

            var activeTopics = entityInfoInternals.Where(e => e.Namespace.Mode == NamespaceMode.Active).GroupBy(e => e.Namespace.Alias)
                .Select(g => g.First());

            return new TopologySectionInternal
            {
                Entities = passiveTopics.Union(activeTopics),
                Namespaces = namespaces
            };
        }

        TopologySectionInternal CreateSectionForSend(string destination)
        {
            var inputQueueAddress = addressingLogic.Apply(destination, EntityType.Queue);

            RuntimeNamespaceInfo[] namespaces = null;
            if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultNameSpaceAlias) // sending to specific namespace
            {
                if (inputQueueAddress.HasConnectionString)
                {
                    namespaces = new[]
                    {
                        new RuntimeNamespaceInfo(inputQueueAddress.Suffix, inputQueueAddress.Suffix, NamespacePurpose.Routing)
                    };
                }
                else
                {
                    NamespaceInfo configured = null;
                    foreach (var namespaceConfiguration in namespaceConfigurations)
                    {
                        if (namespaceConfiguration.Alias == inputQueueAddress.Suffix)
                        {
                            configured = namespaceConfiguration;
                        }
                    }
                    if (configured != null)
                    {
                        namespaces = new[]
                        {
                            new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose)
                        };
                    }
                }
            }
            else
            {
                NamespaceInfo configured = null;
                foreach (var namespaceConfiguration in namespaceConfigurations)
                {
                    if (namespaceConfiguration.RegisteredEndpoints.Contains(destination, StringComparer.OrdinalIgnoreCase))
                    {
                        configured = namespaceConfiguration;
                    }
                }

                if (configured != null)
                {
                    namespaces = new[]
                    {
                        new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose)
                    };
                }
                else // sending to the partition
                {
                    namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
                }
            }

            if (namespaces == null)
            {
                throw new Exception($"Could not determine namespace for destination `{destination}`.");
            }

            var entities = new List<EntityInfoInternal>(namespaces.Length);
            foreach (var n in namespaces)
            {
                entities.Add(new EntityInfoInternal
                {
                    Path = inputQueueAddress.Name,
                    Type = EntityType.Queue,
                    Namespace = n
                });
            }

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        TopologySectionInternal BuildSubscriptionHierarchy(Type eventType, string localAddress)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            // Using localAddress that will be provided by SubscriptionManager instead of the endpoint name.
            // Reason: endpoint name can be overridden. If the endpoint name is overridden, "originalEndpointName" will not have the override value.
            var sanitizedInputQueuePath = addressingLogic.Apply(localAddress, EntityType.Queue).Name;
            var sanitizedSubscriptionPath = addressingLogic.Apply(localAddress, EntityType.Subscription).Name;

            // rule name needs to be 1) based on event full name 2) unique 3) deterministic
            var ruleName = addressingLogic.Apply(eventType.FullName, EntityType.Rule).Name;

            var subs = new List<SubscriptionInfoInternal>();
            var topics = CreateTopics(namespaces);
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
                            Description = $"Events {originalEndpointName} is subscribed to",
                            SubscriptionNameBasedOnEventWithNamespace = ruleName,
                            NamespaceInfo = ns,
                            SubscribedEventFullName = eventType.FullName
                        },
                        BrokerSideFilter = brokerSideSubscriptionFilterFactory.Create(eventType),
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

        readonly ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();
        readonly ConcurrentDictionary<string, TopologySectionInternal> sendDestinations = new ConcurrentDictionary<string, TopologySectionInternal>();
        readonly ConcurrentDictionary<Type, TopologySectionInternal> publishDestinations = new ConcurrentDictionary<Type, TopologySectionInternal>();
        string originalEndpointName;
        INamespacePartitioningStrategy namespacePartitioningStrategy;
        AddressingLogic addressingLogic;
        string defaultNameSpaceAlias;
        NamespaceConfigurations namespaceConfigurations;
        ICreateBrokerSideSubscriptionFilter brokerSideSubscriptionFilterFactory;
        int numberOfEntitiesInBundle;
        string bundlePrefix;
    }
}