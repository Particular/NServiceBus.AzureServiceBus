namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    // responsible for creating (part of) the topology
    // part of = only the part that matters to this endpoint

    // note there is some creation logic elsewhere already, those calls should be removed and centralized here

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateTopology
    {
        Task Create(TopologySection topology);
    }

    // TODO: Move into internalized ICreateTopology in v8
    interface ITearDownTopology
    {
        Task TearDown(TopologySection topologySection);
    }
}