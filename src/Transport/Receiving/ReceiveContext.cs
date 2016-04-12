namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Holds on to the the receive metadata
    /// - the original message so that it can be completed or abandoned when processing is done
    /// - the queue where it came from, so that sends can go via that queue to emulate send transactions
    /// </summary>
    public abstract class ReceiveContext
    {
        public IList<Func<Task>> OnComplete { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}