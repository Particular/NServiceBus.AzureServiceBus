namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    class SqlSubscriptionFilter : IBrokerSideSubscriptionFilter
    {
        public SqlSubscriptionFilter(Type eventType)
        {
            this.eventType = eventType;
        }

        public string Serialize()
        {
            return $"[{Headers.EnclosedMessageTypes}] LIKE '%{eventType.FullName}%'";
        }

        Type eventType;
    }
}