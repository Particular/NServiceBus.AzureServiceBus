//namespace NServiceBus
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Transactions;
//    using Azure.Transports.WindowsAzureServiceBus;
//    using Configuration.AdvanceExtensibility;
//    using Features;
//    using NServiceBus.ConsistencyGuarantees;
//    using NServiceBus.Performance.TimeToBeReceived;
//    using Transports;

//    /// <summary>
//    /// Transport definition for WindowsAzureServiceBus    
//    /// </summary>
//    public class AzureServiceBusTransport : TransportDefinition
//    {
//        public AzureServiceBusTransport()
//        {
//            HasNativePubSubSupport = true;
//            HasSupportForCentralizedPubSub = false;
//            HasSupportForDistributedTransactions = false;
//        }

//        /// <summary>
//        /// Gives implementations access to the <see cref="T:NServiceBus.BusConfiguration"/> instance at configuration time.
//        /// </summary>
//        protected override void Configure(BusConfiguration config)
//        {
//            config.GetSettings().SetDefault("SelectedSerializer", new JsonSerializer());
//            config.GetSettings().SetDefault("EndpointInstanceDiscriminator", QueueIndividualizer.Discriminator);

//            // make sure the transaction stays open a little longer than the long poll.
//            config.Transactions().DefaultTimeout(TimeSpan.FromSeconds(AzureServicebusDefaults.DefaultServerWaitTime * 1.1)).IsolationLevel(IsolationLevel.Serializable);

//            config.EnableFeature<AzureServiceBusTransportConfiguration>();
//        }

//        // TODO: should core have this logic where transports only return SubScopeString ("." for ASB, "-" for ASQ)
//        public override string GetSubScope(string address, string qualifier)
//        {
//            return string.Format("{0}.{1}", address, qualifier);
//        }

//        // TODO: Supported-delivery-constraints sounds like "give me all the things you can't support for delivery"
//        // Perhaps should be "GetSupportedDeliveryCapabilities()"?
//        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
//        {
//            return new List<Type>
//            {
//                // TODO: why do we need to use one generalized constraint instead of specifying what support can do? 
//                // Is using DelayedDelivery.DelayedDeliveryConstraint means that all DelayedDelivery methods are supported
//                typeof(DelayedDelivery.DelayedDeliveryConstraint),
//                //typeof(DelayedDelivery.DelayDeliveryWith),
//                //typeof(DelayedDelivery.DoNotDeliverBefore),
//                // TODO: DelayedDelivery.DelayDeliveryWith is technically converted into DelayedDelivery.DoNotDeliverBefore (current datetime + delay span). Why to have the two?

//                typeof(DiscardIfNotReceivedBefore), //TTBR

//                // typeof(NonDurableDelivery) - N/A since we don't support express messages
//            };
//        }

//        public override ConsistencyGuarantee GetDefaultConsistencyGuarantee()
//        {
//            return new AtLeastOnce();
//        }
//    }
//}