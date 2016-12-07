namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;

    class TopologyCreator : ICreateTopologyInternal
    {
        ITransportPartsContainerInternal container;
        IManageNamespaceManagerLifeCycleInternal namespaces;

        public TopologyCreator(ITransportPartsContainerInternal container, IManageNamespaceManagerLifeCycleInternal namespaces)
        {
            this.container = container;
            this.namespaces = namespaces;
        }

        public async Task Create(TopologySectionInternal topology)
        {
            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue).ToList();
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic).ToList();
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription).ToList();

            if (queues.Any())
            {
                var queueCreator = (ICreateAzureServiceBusQueuesInternal)container.Resolve(typeof(ICreateAzureServiceBusQueuesInternal));
                foreach (var queue in queues)
                {
                    await queueCreator.Create(queue.Path, namespaces.Get(queue.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (topics.Any())
            {
                var topicCreator = (ICreateAzureServiceBusTopicsInternal)container.Resolve(typeof(ICreateAzureServiceBusTopicsInternal));
                foreach (var topic in topics)
                {
                    await topicCreator.Create(topic.Path, namespaces.Get(topic.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (subscriptions.Any())
            {
                var subscriptionCreator = (ICreateAzureServiceBusSubscriptionsInternal) container.Resolve(typeof(ICreateAzureServiceBusSubscriptionsInternal));
                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipTypeInternal.Forward);
                    var sqlFilter = (subscription as SubscriptionInfoInternal)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfoInternal)?.Metadata ?? new SubscriptionMetadataInternal();
                    await subscriptionCreator.Create(topic.Target.Path, subscription.Path, metadata, sqlFilter, namespaces.Get(subscription.Namespace.Alias), forwardTo?.Target.Path).ConfigureAwait(false);
                }
            }
        }

        public async Task TearDown(TopologySectionInternal topologySection)
        {
            var subscriptions = topologySection.Entities.Where(e => e.Type == EntityType.Subscription).ToList();
            if (subscriptions.Any())
            {
                var subscriptionCreator = (ICreateAzureServiceBusSubscriptionsInternal)container.Resolve(typeof(ICreateAzureServiceBusSubscriptionsInternal));

                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipTypeInternal.Forward);
                    var sqlFilter = (subscription as SubscriptionInfoInternal)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfoInternal)?.Metadata ?? new SubscriptionMetadataInternal();


                    await subscriptionCreator.DeleteSubscription(topicPath: topic.Target.Path,
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