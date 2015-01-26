namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateSubscriptions
    {
        SubscriptionDescription Create(Address topic, Type eventType, string subscriptionname);
        void Delete(Address topic, string subscriptionname);
    }
}