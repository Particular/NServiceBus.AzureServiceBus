namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
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