namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport;

    interface IBatcherInternal
    {
        Task<List<BatchInternal>> ToBatches(TransportOperations operations);
    }
}