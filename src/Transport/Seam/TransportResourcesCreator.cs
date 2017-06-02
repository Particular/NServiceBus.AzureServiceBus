namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Transport;

    class TransportResourcesCreator : ICreateQueues
    {
        ITopologySectionManagerInternal sections;
        TopologyCreator topologyCreator;
        bool resourcesCreated;

        public TransportResourcesCreator(TopologyCreator topologyCreator, ITopologySectionManagerInternal sections)
        {
            this.topologyCreator = topologyCreator;
            this.sections = sections;
        }

        public async Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            if (resourcesCreated)
            {
                return;
            }

            await topologyCreator.AssertManagedRights().ConfigureAwait(false);

            var receiveResources = await sections.DetermineResourcesToCreate(queueBindings).ConfigureAwait(false);
            await topologyCreator.Create(receiveResources).ConfigureAwait(false);

            resourcesCreated = true;
        }
    }
}