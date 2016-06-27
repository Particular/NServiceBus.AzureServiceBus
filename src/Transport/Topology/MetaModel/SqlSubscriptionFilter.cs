namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    class SqlSubscriptionFilter : IBrokerSideSubscriptionFilter
    {
        Type eventType;

        public SqlSubscriptionFilter(Type eventType)
        {
            this.eventType = eventType;
        }

        public string Serialize()
        {
            return string.Format("[{0}] LIKE '{1}%' OR [{0}] LIKE '%{1}%' OR [{0}] LIKE '%{1}' OR [{0}] = '{1}'", Headers.EnclosedMessageTypes, eventType.FullName);
        }

    }
}