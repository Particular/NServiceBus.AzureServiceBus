namespace NServiceBus.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.ObjectBuilder.Common;

    class TopologyCreator : ICreateTopology
    {
        readonly IContainer container;
        readonly IManageNamespaceManagerLifeCycle namespaces;

        public TopologyCreator(IContainer container, IManageNamespaceManagerLifeCycle namespaces)
        {
            this.container = container;
            this.namespaces = namespaces;
        }

        public async Task Create(TopologyDefinition topology)
        {
            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue);
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic);
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription);

            var queueCreator = (ICreateAzureServiceBusQueues)container.Build(typeof(ICreateAzureServiceBusQueues));
            foreach (var queue in queues)
            {
                await queueCreator.CreateAsync(queue.Path, namespaces.Get(queue.Namespace.ConnectionString));
            }

            var topicCreator = (ICreateAzureServiceBusTopics)container.Build(typeof(ICreateAzureServiceBusTopics));
            foreach (var topic in topics)
            {
                await topicCreator.CreateAsync(topic.Path, namespaces.Get(topic.Namespace.ConnectionString));
            }

            var subscriptionCreator = (ICreateAzureServiceBusSubscriptions)container.Build(typeof(ICreateAzureServiceBusSubscriptions));
            foreach (var subscription in subscriptions)
            {
                var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                await subscriptionCreator.CreateAsync(topic.Target.Path, subscription.Path, namespaces.Get(subscription.Namespace.ConnectionString));
            }
        }
    }
}