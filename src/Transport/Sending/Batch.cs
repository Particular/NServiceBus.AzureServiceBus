namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    class BatchInternal
    {
        public BatchInternal()
        {
            Operations = new List<BatchedOperationInternal>();
        }

        public TopologySectionInternal Destinations { get; set; }

        public DispatchConsistency RequiredDispatchConsistency { get; set; }

        public IList<BatchedOperationInternal> Operations { get; set; }
    }
}