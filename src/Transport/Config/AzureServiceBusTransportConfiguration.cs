namespace NServiceBus.Features
{
    using System;
    using Azure.Transports.WindowsAzureServiceBus;
    using Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using Config;
    using Microsoft.ServiceBus;
    using Settings;
    using Transports;

    class AzureServiceBusTransportConfiguration : ConfigureTransport
    {
        internal AzureServiceBusTransportConfiguration()
        {
            Defaults(a =>
            {
               
            });
        }

        protected override string GetLocalAddress(ReadOnlySettings settings)
        {
            var configSection = settings.GetConfigSection<AzureServiceBusQueueConfig>();
            if (configSection == null)
            {
                //hack: just to get the defaults, we should refactor this to support specifying the values on the NServiceBus/Transport connection string as well
                configSection = new AzureServiceBusQueueConfig();
            }

            ServiceBusEnvironment.SystemConnectivity.Mode = (ConnectivityMode)Enum.Parse(typeof(ConnectivityMode), configSection.ConnectivityMode);

            return NamingConventions.QueueNamingConvention(settings, null, settings.Get<string>("NServiceBus.LocalAddress"), false);
            
        }

        protected override void Configure(FeatureConfigurationContext context, string defaultconnectionString)
        {
            var bestConnectionString = new DeterminesBestConnectionStringForAzureServiceBus(defaultconnectionString).Determine(context.Settings);

            // this is  a bug in the core, statics reused across tests
            try // would work on IWantToRunBeforeConfiguration, but would be better to move this method to base configuretransport
            {
                Address.OverrideDefaultMachine(bestConnectionString);
            }
            catch (InvalidOperationException)
            {
                // yes, testing warrants it
            }
        }

        protected override bool RequiresConnectionString
        {
            get { return false; }
        }

        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return "Endpoint=sb://{yournamespace}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={yourkey}"; }
        }
    }

    
}