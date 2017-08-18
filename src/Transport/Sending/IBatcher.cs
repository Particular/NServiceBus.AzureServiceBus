namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    interface IBatcherInternal
    {
        IList<BatchInternal> ToBatches(TransportOperations operations);
    }
}