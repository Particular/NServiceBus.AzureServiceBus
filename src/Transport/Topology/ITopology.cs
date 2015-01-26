namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using Settings;

    public interface ITopology
    {
        void Initialize(ReadOnlySettings setting);

        INotifyReceivedBrokeredMessages Subscribe(Type eventType, Address address);
        void Unsubscribe(INotifyReceivedBrokeredMessages notifier);

        INotifyReceivedBrokeredMessages GetReceiver(Address address);

        ISendBrokeredMessages GetSender(Address destination);
        IPublishBrokeredMessages GetPublisher(Address local);
        void Create(Address address);
    }
}
