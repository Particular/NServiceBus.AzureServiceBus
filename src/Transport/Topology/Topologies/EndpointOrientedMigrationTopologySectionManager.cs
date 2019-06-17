namespace NServiceBus.Transport.AzureServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Connectivity;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    class EndpointOrientedMigrationTopologySectionManager : ITopologySectionManagerInternal
    {
        public EndpointOrientedMigrationTopologySectionManager(string defaultNameSpaceAlias, NamespaceConfigurations namespaceConfigurations, string originalEndpointName, PublishersConfiguration publishersConfiguration, INamespacePartitioningStrategy namespacePartitioningStrategy, AddressingLogic addressingLogic, ICreateBrokerSideSubscriptionFilter brokerSideSubscriptionFilterFactory)
        {
            this.namespaceConfigurations = namespaceConfigurations;
            this.defaultNameSpaceAlias = defaultNameSpaceAlias;
            this.originalEndpointName = originalEndpointName;
            this.addressingLogic = addressingLogic;
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.publishersConfiguration = publishersConfiguration;
            this.brokerSideSubscriptionFilterFactory = brokerSideSubscriptionFilterFactory;
        }

        public Func<Task> Initialize { get; set; } = () => TaskEx.Completed;

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
            var entities = new List<EntityInfoInternal>();

            foreach (var n in namespaces)
            {
                // legacy topic
                entities.Add(new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(localAddress + ".events", EntityType.Topic).Name,
                    Type = EntityType.Topic,
                    Namespace = n
                });
                // bundle from forwarding
                entities.Add(new EntityInfoInternal
                {
                    Path = "bundle-1",
                    Type = EntityType.Topic,
                    Namespace = n
                });
                // migration
                // Dedup is defined in custom descriptor override, ugly yes but necessary evil
                entities.Add(new EntityInfoInternal
                {
                    Path = MigrationTopicName,
                    Type = EntityType.Topic,
                    Namespace = n
                });
                var subscription = new SubscriptionInfoInternal
                {
                    Namespace = n,
                    Type = EntityType.Subscription,
                    Path = MigrationTopicName,
                    Metadata = new SubscriptionMetadataInternal
                    {
                        Description = "Forwarding to bundle-1",
                        SubscriptionNameBasedOnEventWithNamespace = MigrationTopicName
                    },
                    BrokerSideFilter = brokerSideSubscriptionFilterFactory.CreateCatchAll(),
                    ShouldBeListenedTo = false
                };
                subscription.RelationShips.Add(new EntityRelationShipInfoInternal
                {
                    Source = subscription,
                    Target = new EntityInfoInternal
                    {
                        Namespace = n,
                        Path = MigrationTopicName,
                        Type = EntityType.Topic
                    },
                    Type = EntityRelationShipTypeInternal.Subscription
                });
                subscription.RelationShips.Add(new EntityRelationShipInfoInternal
                {
                    Source = subscription,
                    Target = new EntityInfoInternal
                    {
                        Namespace = n,
                        Path = "bundle-1",
                        Type = EntityType.Topic
                    },
                    Type = EntityRelationShipTypeInternal.Forward
                });

                entities.Add(subscription);
            }

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
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

                // assumed errorq and auditq are in here
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
            return namespacePartitioningStrategy.SendingNamespacesCanBeCached ? publishDestinations.GetOrAdd(eventType, t => CreateSectionForPublish(localAddress)) : CreateSectionForPublish(localAddress);
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

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            if (!subscriptions.TryRemove(eventtype, out var result))
            {
                result = new TopologySectionInternal
                {
                    Entities = new List<SubscriptionInfoInternal>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }

        TopologySectionInternal CreateSectionForPublish(string localAddress)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();

            var topicPath = addressingLogic.Apply($"{localAddress}.events", EntityType.Topic).Name;
            var entities = new List<EntityInfoInternal>();

            foreach (var n in namespaces)
            {
                entities.Add(new EntityInfoInternal
                {
                    Path = topicPath,
                    Type = EntityType.Topic,
                    Namespace = n
                });
            }

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities
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

            var publishers = publishersConfiguration.GetPublishersFor(eventType);

            // Using localAddress that will be provided by SubscriptionManager instead of the endpoint name.
            // Reason: endpoint name can be overridden. If the endpoint name is overridden, "originalEndpointName" will not have the override value.
            var subscriptionNameCandidateV6 = $"{localAddress}.{eventType.Name}";
            var subscriptionNameV6 = addressingLogic.Apply(subscriptionNameCandidateV6, EntityType.Subscription).Name;
            var subscriptionNameCandidate = $"{localAddress}.{eventType.FullName}";
            var subscriptionName = addressingLogic.Apply(subscriptionNameCandidate, EntityType.Subscription).Name;

            // Using localAddress that will be provided by SubscriptionManager instead of the endpoint name.
            // Reason: endpoint name can be overridden. If the endpoint name is overridden, "originalEndpointName" will not have the override value.
            var sanitizedInputQueuePath = addressingLogic.Apply(localAddress, EntityType.Queue).Name;
            var sanitizedSubscriptionPath = addressingLogic.Apply(localAddress, EntityType.Subscription).Name;

            var ruleName = addressingLogic.Apply(eventType.FullName, EntityType.Rule).Name;

            var topics = new List<EntityInfoInternal>();
            var subs = new List<SubscriptionInfoInternal>();

            foreach (var publisher in publishers)
            {
                var topicPath = $"{publisher}.events";
                var path = addressingLogic.Apply(topicPath, EntityType.Topic).Name;

                var destinationsOutsideTopology = namespaceConfigurations.Where(c => c.RegisteredEndpoints.Contains(publisher, StringComparer.OrdinalIgnoreCase)).ToList();
                if (destinationsOutsideTopology.Any())
                {
                    // It's important to create Forwarding topology subscription infrastructure first in order not to lose messages in flight auto-forwarded by subscriptions under Endpoint-oriented topology
                    CreateForwardingTopologyPart(eventType, subs, namespaces, sanitizedSubscriptionPath, ruleName, sanitizedInputQueuePath);

                    topics.AddRange(destinationsOutsideTopology.Select(ns => new EntityInfoInternal
                    {
                        Namespace = new RuntimeNamespaceInfo(ns.Alias, ns.Connection, NamespacePurpose.Routing),
                        Type = EntityType.Topic,
                        Path = path
                    }));

                    subs.AddRange(destinationsOutsideTopology.Select(ns =>
                    {
                        var rns = new RuntimeNamespaceInfo(ns.Alias, ns.Connection, NamespacePurpose.Routing);
                        var sub = new SubscriptionInfoInternal
                        {
                            Namespace = rns,
                            Type = EntityType.Subscription,
                            Path = subscriptionNameV6,
                            Metadata = new SubscriptionMetadataInternal
                            {
                                Description = $"{originalEndpointName} subscribed to {eventType.FullName}",
                                SubscriptionNameBasedOnEventWithNamespace = subscriptionName
                            },
                            BrokerSideFilter = brokerSideSubscriptionFilterFactory.Create(eventType),
                            ShouldBeListenedTo = false
                        };
                        sub.RelationShips.Add(new EntityRelationShipInfoInternal
                        {
                            Source = sub,
                            Target = topics.First(t => t.Path == path && t.Namespace == rns),
                            Type = EntityRelationShipTypeInternal.Subscription
                        });
                        sub.RelationShips.Add(new EntityRelationShipInfoInternal
                        {
                            Source = sub,
                            Target = new EntityInfoInternal
                            {
                                Namespace = new RuntimeNamespaceInfo(ns.Alias, ns.Connection),
                                Path = MigrationTopicName,
                                Type = EntityType.Topic
                            },
                            Type = EntityRelationShipTypeInternal.Forward
                        });
                        return sub;
                    }));
                }
                else
                {
                    // It's important to create Forwarding topology subscription infrastructure first in order not to lose messages in flight auto-forwarded by subscriptions under Endpoint-oriented topology
                    CreateForwardingTopologyPart(eventType, subs, namespaces, sanitizedSubscriptionPath, ruleName, sanitizedInputQueuePath);

                    topics.AddRange(namespaces.Select(ns => new EntityInfoInternal
                    {
                        Namespace = ns,
                        Type = EntityType.Topic,
                        Path = path
                    }));

                    subs.AddRange(namespaces.Select(ns =>
                    {
                        var sub = new SubscriptionInfoInternal
                        {
                            Namespace = ns,
                            Type = EntityType.Subscription,
                            Path = subscriptionNameV6,
                            Metadata = new SubscriptionMetadataInternal
                            {
                                Description = $"{originalEndpointName} subscribed to {eventType.FullName}",
                                SubscriptionNameBasedOnEventWithNamespace = subscriptionName
                            },
                            BrokerSideFilter = brokerSideSubscriptionFilterFactory.Create(eventType),
                            ShouldBeListenedTo = false
                        };
                        sub.RelationShips.Add(new EntityRelationShipInfoInternal
                        {
                            Source = sub,
                            Target = topics.First(t => t.Path == path && t.Namespace == ns),
                            Type = EntityRelationShipTypeInternal.Subscription
                        });
                        sub.RelationShips.Add(new EntityRelationShipInfoInternal
                        {
                            Source = sub,
                            Target = new EntityInfoInternal
                            {
                                Namespace = ns,
                                Path = MigrationTopicName,
                                Type = EntityType.Topic
                            },
                            Type = EntityRelationShipTypeInternal.Forward
                        });

                        return sub;
                    }));
                }
            }

            return new TopologySectionInternal
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }

        void CreateForwardingTopologyPart(Type eventType, List<SubscriptionInfoInternal> subs, RuntimeNamespaceInfo[] namespaces, string sanitizedSubscriptionPath, string ruleName, string sanitizedInputQueuePath)
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
                    Target = new EntityInfoInternal
                    {
                        Namespace = ns,
                        Path = "bundle-1",
                        Type = EntityType.Topic
                    },
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

        ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();
        ConcurrentDictionary<string, TopologySectionInternal> sendDestinations = new ConcurrentDictionary<string, TopologySectionInternal>();
        ConcurrentDictionary<Type, TopologySectionInternal> publishDestinations = new ConcurrentDictionary<Type, TopologySectionInternal>();
        INamespacePartitioningStrategy namespacePartitioningStrategy;
        AddressingLogic addressingLogic;
        PublishersConfiguration publishersConfiguration;
        string originalEndpointName;
        string defaultNameSpaceAlias;
        NamespaceConfigurations namespaceConfigurations;
        ICreateBrokerSideSubscriptionFilter brokerSideSubscriptionFilterFactory;
        public const string MigrationTopicName = "migration";
    }
}