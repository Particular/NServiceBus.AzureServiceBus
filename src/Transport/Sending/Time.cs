namespace NServiceBus.AzureServiceBus
{
    using System;

    internal static class Time
    {
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}