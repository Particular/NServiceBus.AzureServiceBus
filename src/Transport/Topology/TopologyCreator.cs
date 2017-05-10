namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using NServiceBus.AzureServiceBus.Utils;
    using Settings;

    class TopologyCreator : ICreateTopology, ITearDownTopology
    {
        static ILog Logger = LogManager.GetLogger<TopologyCreator>();
        ITransportPartsContainer container;
        IManageNamespaceManagerLifeCycle namespaces;
        AsyncLazy<bool> hasManagedRights;
        string namespacesWithoutManagedRightsJoined;

        public TopologyCreator(ITransportPartsContainer container, IManageNamespaceManagerLifeCycle namespaces)
        {
            this.container = container;
            this.namespaces = namespaces;
            hasManagedRights = new AsyncLazy<bool>(async () =>
            {
                var namespacesWithoutManagedRights = await ManageRightsCheck.Run(namespaces, container.Resolve<ReadOnlySettings>())
                    .ConfigureAwait(false);
                namespacesWithoutManagedRightsJoined = string.Join(", ", namespacesWithoutManagedRights.Select(alias => $"`{alias}`"));
                return namespacesWithoutManagedRights.Count == 0;
            });
        }

        public async Task Create(TopologySection topology)
        {
            if (!await hasManagedRights)
            {
                Logger.Info($"Configured to create topology, but have no manage rights for the following namespace(s): {namespacesWithoutManagedRightsJoined}. Execution will continue and assume the topology is already created.");
                return;
            }

            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue).ToList();
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic).ToList();
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription).ToList();

            if (queues.Any())
            {
                var queueCreator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
                foreach (var queue in queues)
                {
                    await queueCreator.Create(queue.Path, namespaces.Get(queue.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (topics.Any())
            {
                var topicCreator = (ICreateAzureServiceBusTopics)container.Resolve(typeof(ICreateAzureServiceBusTopics));
                foreach (var topic in topics)
                {
                    await topicCreator.Create(topic.Path, namespaces.Get(topic.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (subscriptions.Any())
            {
                var subscriptionCreator = (ICreateAzureServiceBusSubscriptions) container.Resolve(typeof(ICreateAzureServiceBusSubscriptions));
                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipType.Forward);
                    var sqlFilter = (subscription as SubscriptionInfo)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfo)?.Metadata ?? new SubscriptionMetadata();
                    await subscriptionCreator.Create(topic.Target.Path, subscription.Path, metadata, sqlFilter, namespaces.Get(subscription.Namespace.Alias), forwardTo?.Target.Path).ConfigureAwait(false);
                }
            }
        }

        public async Task TearDown(TopologySection topologySection)
        {
            var subscriptions = topologySection.Entities.Where(e => e.Type == EntityType.Subscription).ToList();
            if (subscriptions.Any())
            {
                var subscriptionCreator = (ICreateAzureServiceBusSubscriptions)container.Resolve(typeof(ICreateAzureServiceBusSubscriptions));

                var subscriptionCreatorAbleToDeleteSubscriptions = subscriptionCreator as ICreateAzureServiceBusSubscriptionsAbleToDeleteSubscriptions;
                if (subscriptionCreatorAbleToDeleteSubscriptions == null)
                {
                    return;
                }

                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipType.Forward);
                    var sqlFilter = (subscription as SubscriptionInfo)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfo)?.Metadata ?? new SubscriptionMetadata();


                    await subscriptionCreatorAbleToDeleteSubscriptions.DeleteSubscription(topicPath: topic.Target.Path, 
                        subscriptionName: subscription.Path, 
                        metadata: metadata, 
                        sqlFilter: sqlFilter, 
                        namespaceManager: namespaces.Get(subscription.Namespace.Alias), 
                        forwardTo: forwardTo?.Target.Path).ConfigureAwait(false);
                }
            }
        }
    }
}