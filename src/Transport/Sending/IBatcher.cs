namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using NServiceBus.Transports;

    public interface IBatcher
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }
}