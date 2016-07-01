namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;

    public class AzureServiceBusTransportConfigContext
    {
        public Action<string, TransportExtensions< AzureServiceBusTransport>> Callback
        {
            get;
            set;
        }
    }
}