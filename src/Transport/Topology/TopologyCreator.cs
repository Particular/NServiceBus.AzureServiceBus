namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;

    class TopologyCreator : ICreateTopologyInternal
    {
        public TopologyCreator(ICreateAzureServiceBusSubscriptionsInternal subscriptionsCreator, AzureServiceBusQueueCreator queuesCreator, AzureServiceBusTopicCreator topicsCreator, IManageNamespaceManagerLifeCycleInternal namespaces)
        {
            this.topicsCreator = topicsCreator;
            this.queuesCreator = queuesCreator;
            this.subscriptionsCreator = subscriptionsCreator;
            this.namespaces = namespaces;
        }

        public async Task Create(TopologySectionInternal topology)
        {
            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue).ToList();
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic).ToList();
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription).ToList();

            if (queues.Any())
            {
                foreach (var queue in queues)
                {
                    await queuesCreator.Create(queue.Path, namespaces.Get(queue.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (topics.Any())
            {
                foreach (var topic in topics)
                {
                    await topicsCreator.Create(topic.Path, namespaces.Get(topic.Namespace.Alias)).ConfigureAwait(false);
                }
            }

            if (subscriptions.Any())
            {
                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipTypeInternal.Forward);
                    var sqlFilter = (subscription as SubscriptionInfoInternal)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfoInternal)?.Metadata ?? new SubscriptionMetadataInternal();
                    await subscriptionsCreator.Create(topic.Target.Path, subscription.Path, metadata, sqlFilter, namespaces.Get(subscription.Namespace.Alias), forwardTo?.Target.Path).ConfigureAwait(false);
                }
            }
        }

        public async Task TearDown(TopologySectionInternal topologySection)
        {
            var subscriptions = topologySection.Entities.Where(e => e.Type == EntityType.Subscription).ToList();
            if (subscriptions.Any())
            {
                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                    var forwardTo = subscription.RelationShips.FirstOrDefault(r => r.Type == EntityRelationShipTypeInternal.Forward);
                    var sqlFilter = (subscription as SubscriptionInfoInternal)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfoInternal)?.Metadata ?? new SubscriptionMetadataInternal();


                    await subscriptionsCreator.DeleteSubscription(topicPath: topic.Target.Path,
                        subscriptionName: subscription.Path,
                        metadata: metadata,
                        sqlFilter: sqlFilter,
                        namespaceManager: namespaces.Get(subscription.Namespace.Alias),
                        forwardTo: forwardTo?.Target.Path).ConfigureAwait(false);
                }
            }
        }

        IManageNamespaceManagerLifeCycleInternal namespaces;
        ICreateAzureServiceBusSubscriptionsInternal subscriptionsCreator;
        AzureServiceBusQueueCreator queuesCreator;
        AzureServiceBusTopicCreator topicsCreator;
    }
}