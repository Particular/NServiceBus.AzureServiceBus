namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class EndpointOrientedTopologySectionManager : ITopologySectionManagerInternal
    {
        public EndpointOrientedTopologySectionManager(string defaultNameSpaceAlias, NamespaceConfigurations namespaceConfigurations, string originalEndpointName, PublishersConfiguration publishersConfiguration, INamespacePartitioningStrategy namespacePartitioningStrategy, AddressingLogic addressingLogic)
        {
            this.namespaceConfigurations = namespaceConfigurations;
            this.defaultNameSpaceAlias = defaultNameSpaceAlias;
            this.originalEndpointName = originalEndpointName;
            this.addressingLogic = addressingLogic;
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.publishersConfiguration = publishersConfiguration;
        }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings, string localAddress)
        {
            // computes the topologySectionManager

            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(localAddress, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            var topicPath = addressingLogic.Apply(localAddress + ".events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfoInternal
            {
                Path = topicPath,
                Type = EntityType.Topic,
                Namespace = n
            }).ToArray();
            entities.AddRange(topics);

            foreach (var n in namespaces)
            {
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
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DeterminePublishDestination(Type eventType)
        {
            return publishDestinations.GetOrAdd(eventType, t =>
            {
                var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();
            // TODO: does this need to be localAddress and not endpointOriginalName?
            	var topicPath = addressingLogic.Apply(originalEndpointName + ".events", EntityType.Topic).Name;
                var topics = namespaces.Select(n => new EntityInfoInternal
                {
                    Path = topicPath,
                    Type = EntityType.Topic,
                    Namespace = n
                }).ToArray();

                return new TopologySectionInternal
                {
                    Namespaces = namespaces,
                    Entities = topics
                };
            });
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
                            new RuntimeNamespaceInfo(inputQueueAddress.Suffix, inputQueueAddress.Suffix, NamespacePurpose.Routing)
                        };
                    }
                    else
                    {
                        var configured = namespaceConfigurations.FirstOrDefault(n => n.Alias == inputQueueAddress.Suffix);
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
                    var namespaceToRouteTo = namespaceConfigurations.FirstOrDefault(n => n.RegisteredEndpoints.Contains(d, StringComparer.OrdinalIgnoreCase));
                    if (namespaceToRouteTo != null)
                    {
                        namespaces = new[]
                        {
                            new RuntimeNamespaceInfo(namespaceToRouteTo.Alias, namespaceToRouteTo.Connection, namespaceToRouteTo.Purpose),
                        };
                    }
                    else // sending to the partition
                    {
                        namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
                    }
                }

                if (namespaces == null)
                {
                    throw new Exception($"Could not determine namespace for destination '{d}'");
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

        TopologySectionInternal BuildSubscriptionHierarchy(Type eventType, string localAddress)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var topicPaths = DetermineTopicsFor(eventType);

            // Using localAddress that will be provided by SubscriptionManager instead of the endpoint name.
            // Reason: endpoint name can be overridden. If the endpoint name is overridden, "originalEndpointName" will not have the override value.
            var subscriptionNameCandidateV6 = localAddress + "." + eventType.Name;
            var subscriptionNameV6 = addressingLogic.Apply(subscriptionNameCandidateV6, EntityType.Subscription).Name;
            var subscriptionNameCandidate = localAddress + "." + eventType.FullName;
            var subscriptionName = addressingLogic.Apply(subscriptionNameCandidate, EntityType.Subscription).Name;

            var topics = new List<EntityInfoInternal>();
            var subs = new List<SubscriptionInfoInternal>();
            foreach (var topicPath in topicPaths)
            {
                var path = addressingLogic.Apply(topicPath, EntityType.Topic).Name;
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
                            Description = originalEndpointName + " subscribed to " + eventType.FullName,
                            SubscriptionNameBasedOnEventWithNamespace = subscriptionName
                        },
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType),
                        ShouldBeListenedTo = true
                    };
                    sub.RelationShips.Add(new EntityRelationShipInfoInternal
                    {
                        Source = sub,
                        Target = topics.First(t => t.Path == path && t.Namespace == ns),
                        Type = EntityRelationShipTypeInternal.Subscription
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

        List<string> DetermineTopicsFor(Type eventType)
        {
            return publishersConfiguration
                .GetPublishersFor(eventType)
                .Select(x => string.Concat(x, ".events"))
                .ToList();
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
    }
}