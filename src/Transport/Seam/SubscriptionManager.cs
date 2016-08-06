namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Transport;

    class SubscriptionManager : IManageSubscriptions
    {
        ITopologySectionManager topologySectionManager; // responsible for providing the metadata about the subscription (what in case of EH?)
        IOperateTopology topologyOperator; // responsible for operating the subscription (creating if needed & receiving from)
        ICreateTopology topologyCreator;

        public SubscriptionManager(ITopologySectionManager topologySectionManager, IOperateTopology topologyOperator, ICreateTopology topologyCreator)
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

        public Task Unsubscribe(Type eventType, ContextBag context)
        {
            var section = topologySectionManager.DetermineResourcesToUnsubscribeFrom(eventType);
            return topologyOperator.Stop(section.Entities);
        }
    }
}