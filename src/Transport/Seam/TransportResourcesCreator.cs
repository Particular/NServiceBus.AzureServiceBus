namespace NServiceBus.AzureServiceBus
{
    using NServiceBus.Transports;

    class TransportResourcesCreator : ICreateQueues
    {
        ITopologySectionManager sections;
        ICreateTopology topologyCreator;
        bool resourcesCreated = false;

        public TransportResourcesCreator(ICreateTopology topologyCreator, ITopologySectionManager sections)
        {
            this.topologyCreator = topologyCreator;
            this.sections = sections;
        }

        public void CreateQueueIfNecessary(string address, string account)
        {
            if (resourcesCreated) return;

           var receiveResources = sections.DetermineResourcesToCreate();
           topologyCreator.Create(receiveResources).GetAwaiter().GetResult();

            resourcesCreated = true;
        }
    }
}