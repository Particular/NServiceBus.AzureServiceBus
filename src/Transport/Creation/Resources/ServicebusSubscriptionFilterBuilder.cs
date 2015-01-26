namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;

    class ServicebusSubscriptionFilterBuilder
    {
        public string BuildFor(Type eventType)
        {
            return string.Format("[{0}] LIKE '{1}%' OR [{0}] LIKE '%{1}%' OR [{0}] LIKE '%{1}' OR [{0}] = '{1}'", Headers.EnclosedMessageTypes, eventType.FullName);
        }
    }
}