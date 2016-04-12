namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Transports;

    public interface IBatcher
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }
}