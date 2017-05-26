namespace NServiceBus.AcceptanceTests
{
    using AcceptanceTesting.Support;

    public partial class TestSuiteConstraints
    {
        public bool SupportsDtc { get; } = false;
        public bool SupportsCrossQueueTransactions { get; } = true;
        public bool SupportsNativePubSub { get; } = true;
        public bool SupportsNativeDeferral { get; } = true;
        public bool SupportsOutbox { get; } = false;
     
        public IConfigureEndpointTestExecution CreatePersistenceConfiguration() => new ConfigureEndpointAzureServiceBusTransport();

        public IConfigureEndpointTestExecution CreateTransportConfiguration() => new ConfigureEndpointInMemoryPersistence();
    }
}