namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    public interface INotifyReceivedBrokeredMessages
    {
        void Start(Action<BrokeredMessage> tryProcessMessage, Action<Exception> errorProcessingMessage);

        void Stop();

        event EventHandler Faulted;

        Type MessageType { get; set; }
        Address Address { get; set; }
    }
}