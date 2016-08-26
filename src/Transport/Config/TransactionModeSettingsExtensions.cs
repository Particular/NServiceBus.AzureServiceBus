namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    static class TransactionModeSettingsExtensions
    {
        public static TransportTransactionMode SupportedTransactionMode(this ReadOnlySettings settings)
        {
            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var sendVia = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);
            var transportType = settings.Get<TransportType>(WellKnownConfigurationKeys.Connectivity.TransportType);
            var receiveMode = settings.Get<ReceiveMode>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode);

            if (namespaces.Count == 1 && sendVia && transportType == TransportType.NetMessaging)
                return TransportTransactionMode.SendsAtomicWithReceive;

            return receiveMode == ReceiveMode.PeekLock ? TransportTransactionMode.ReceiveOnly : TransportTransactionMode.None;
        }
    }
}