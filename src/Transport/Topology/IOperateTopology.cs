namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Operational aspects of running on top of the topology
    /// Takes care of the topology and it's specific state at runtime
    /// Examples
    /// Decisions of currently active namespace go here f.e.
    /// So is the list of notifiers etc...
    /// etc..
    /// </summary>
    public interface IOperateTopology
    {
        Task Start(IEnumerable<EntityInfo> subscriptions);
        Task Stop(IEnumerable<EntityInfo> subscriptions);
    }
}