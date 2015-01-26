namespace NServiceBus
{
    using System;
    using System.Transactions;
    using Azure.Transports.WindowsAzureServiceBus;
    using Configuration.AdvanceExtensibility;
    using Features;
    using Transports;

    /// <summary>
    /// Transport definition for WindowsAzureServiceBus    
    /// </summary>
    public class AzureServiceBusTransport : TransportDefinition
    {
        public AzureServiceBusTransport()
        {
            HasNativePubSubSupport = true;
            HasSupportForCentralizedPubSub = false;
            HasSupportForDistributedTransactions = false;
        }

        /// <summary>
        /// Gives implementations access to the <see cref="T:NServiceBus.BusConfiguration"/> instance at configuration time.
        /// </summary>
        protected override void Configure(BusConfiguration config)
        {
            config.GetSettings().SetDefault("SelectedSerializer", new JsonSerializer());
            config.GetSettings().SetDefault("EndpointInstanceDiscriminator", QueueIndividualizer.Discriminator);

            // make sure the transaction stays open a little longer than the long poll.
            config.Transactions().DefaultTimeout(TimeSpan.FromSeconds(AzureServicebusDefaults.DefaultServerWaitTime * 1.1)).IsolationLevel(IsolationLevel.Serializable);

            config.EnableFeature<AzureServiceBusTransportConfiguration>();
        }
    }
}