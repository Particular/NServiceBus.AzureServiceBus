namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    interface IBatcherInternal
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }
}