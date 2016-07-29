namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport;

    public interface IRouteOutgoingBatches
    {
        Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContext receiveContext, DispatchConsistency consistency);
    }
}