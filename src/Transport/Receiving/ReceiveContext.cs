namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading;
    using System.Transactions;

    /// <summary>
    /// Holds on to the the receive metadata
    /// - the original message so that it can be completed or abandoned when processing is done
    /// - the queue where it came from, so that sends can go via that queue to emulate send transactions
    /// </summary>
    public abstract class ReceiveContext
    {
        protected ReceiveContext()
        {
            CancellationToken = CancellationToken.None;
        }

        public CancellationToken CancellationToken { get; internal set; }


        [ObsoleteEx(Message = "Not required.", RemoveInVersion = "9.0", TreatAsErrorFromVersion = "8.0")]
        public CommittableTransaction Transaction { get; set; }
    }
}