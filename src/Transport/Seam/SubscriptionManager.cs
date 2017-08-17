namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;

    class SubscriptionManager : IManageSubscriptions
    {
        public SubscriptionManager(ITopologySectionManagerInternal topologySectionManager, IOperateTopologyInternal topologyOperator, TopologyCreator topologyCreator)
        {
            this.topologySectionManager = topologySectionManager;
            this.topologyOperator = topologyOperator;
            this.topologyCreator = topologyCreator;
        }

        public async Task Subscribe(Type eventType, ContextBag context)
        {
            var section = topologySectionManager.DetermineResourcesToSubscribeTo(eventType);
            await topologyCreator.Create(section).ConfigureAwait(false);
            topologyOperator.Start(section.Entities);
        }

        public async Task Unsubscribe(Type eventType, ContextBag context)
        {
            var section = topologySectionManager.DetermineResourcesToUnsubscribeFrom(eventType);
            await topologyOperator.Stop(section.Entities).ConfigureAwait(false);
            await topologyCreator.TearDown(section).ConfigureAwait(false);
        }

        ITopologySectionManagerInternal topologySectionManager; // responsible for providing the metadata about the subscription (what in case of EH?)
        IOperateTopologyInternal topologyOperator; // responsible for operating the subscription (creating if needed & receiving from)
        TopologyCreator topologyCreator;
    }
}