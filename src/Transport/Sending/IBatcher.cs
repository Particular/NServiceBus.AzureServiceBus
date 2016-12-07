namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    interface IBatcherInternal
    {
        IList<BatchInternal> ToBatches(TransportOperations operations);
    }
}