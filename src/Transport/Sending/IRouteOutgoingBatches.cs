namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRouteOutgoingBatches
    {
        Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContext receiveContext);
        Task RouteBatch(Batch batch, ReceiveContext receiveContext);
    }
}