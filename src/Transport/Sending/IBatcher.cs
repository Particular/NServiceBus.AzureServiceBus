namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    public interface IBatcher
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }
}