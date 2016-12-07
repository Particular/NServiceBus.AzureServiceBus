namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading;

    /// <summary>
    /// Holds on to the the receive metadata
    /// - the original message so that it can be completed or abandoned when processing is done
    /// - the queue where it came from, so that sends can go via that queue to emulate send transactions
    /// </summary>
    abstract class ReceiveContextInternal
    {
        protected ReceiveContextInternal()
        {
            CancellationToken = CancellationToken.None;
        }

        public CancellationToken CancellationToken { get; internal set; }
    }
}