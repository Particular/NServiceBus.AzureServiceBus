namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    // responsible for creating (part of) the topology
    // part of = only the part that matters to this endpoint

    // note there is some creation logic elsewhere already, those calls should be removed and centralized here

    // TODO: remove if not needed (once TopologyCreator is not injected into container)
    interface ICreateTopologyInternal
    {
        Task Create(TopologySectionInternal topology);

        Task TearDown(TopologySectionInternal topologySection);
    }
}