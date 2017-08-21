namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class MessageReceiverNotifierSettings
    {
        public MessageReceiverNotifierSettings(ReceiveMode receiveMode, TransportTransactionMode transportTransactionMode, TimeSpan autoRenewTimeout, int numberOfClients)
        {
            ReceiveMode = receiveMode;
            TransportTransactionMode = transportTransactionMode;
            AutoRenewTimeout = autoRenewTimeout;
            NumberOfClients = numberOfClients;
        }

        public ReceiveMode ReceiveMode { get; }
        public TransportTransactionMode TransportTransactionMode { get; }
        public TimeSpan AutoRenewTimeout { get; }
        public int NumberOfClients { get; }
    }
}