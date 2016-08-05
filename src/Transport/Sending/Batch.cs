namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    public class Batch
    {
        public Batch()
        {
            Operations = new List<BatchedOperation>();
        }

        public TopologySection Destinations { get; set; }

        public DispatchConsistency RequiredDispatchConsistency { get; set; }

        public IList<BatchedOperation> Operations { get; set; }
    }
}