namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IRouteOutgoingBatches
    {
        Task RouteBatches(IEnumerable<Batch> outgoingBatches, ReceiveContext receiveContext, DispatchConsistency consistency);
    }
}