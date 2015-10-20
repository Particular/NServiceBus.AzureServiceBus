namespace NServiceBus
{
    using System;

    public static class ConfigureAzureServiceBusMessageQueue
    {
        [ObsoleteEx(RemoveInVersion = "7", TreatAsErrorFromVersion = "5.4", ReplacementTypeOrMember = "config.UseTransport<AzureServiceBus>()")]
        public static Configure AzureServiceBusMessageQueue(this Configure config)
        {
            throw new InvalidOperationException();
        }
    }
}