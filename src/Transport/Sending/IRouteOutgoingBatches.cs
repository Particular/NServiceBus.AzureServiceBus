namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport;

    interface IRouteOutgoingBatchesInternal
    {
        Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContextInternal receiveContext, DispatchConsistency consistency);
    }
}