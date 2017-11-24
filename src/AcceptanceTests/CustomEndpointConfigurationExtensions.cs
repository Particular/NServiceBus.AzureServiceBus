namespace NServiceBus.AcceptanceTests
{
    using Configuration.AdvanceExtensibility;

    public static class CustomEndpointConfigurationExtensions
    {
        public static TransportExtensions<AzureServiceBusTransport> ConfigureAzureServiceBus(this EndpointConfiguration configuration)
        {
            return new TransportExtensions<AzureServiceBusTransport>(configuration.GetSettings());
        }
    }
}