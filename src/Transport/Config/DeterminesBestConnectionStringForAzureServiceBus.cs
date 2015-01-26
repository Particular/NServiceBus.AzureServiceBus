namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System.Configuration;
    using Config;
    using Settings;

    class DeterminesBestConnectionStringForAzureServiceBus
    {
        string defaultconnectionString;

        public DeterminesBestConnectionStringForAzureServiceBus(string defaultconnectionString)
        {
            this.defaultconnectionString = defaultconnectionString;
        }

        public string Determine(ReadOnlySettings settings)
        {
            var configSection = settings.GetConfigSection<AzureServiceBusQueueConfig>();
            var connectionString = configSection != null ? configSection.ConnectionString : string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = defaultconnectionString;
            }

            if (configSection != null && !string.IsNullOrEmpty(configSection.IssuerKey) && !string.IsNullOrEmpty(configSection.ServiceNamespace))
                connectionString = string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue={1}", configSection.ServiceNamespace, configSection.IssuerKey);

            if (string.IsNullOrEmpty(connectionString) && (configSection == null || string.IsNullOrEmpty(configSection.IssuerKey) || string.IsNullOrEmpty(configSection.ServiceNamespace)))
            {
                throw new ConfigurationErrorsException("No Servicebus Connection information specified, either set the ConnectionString or set the IssuerKey and ServiceNamespace properties");
            }

            return connectionString;
        }

        public bool IsPotentialServiceBusConnectionString(string potentialConnectionString)
        {
            return potentialConnectionString.StartsWith("Endpoint=sb://");
        }

        public string Determine(ReadOnlySettings settings, Address replyToAddress)
        {
            if (IsPotentialServiceBusConnectionString(replyToAddress.Machine))
            {
                return replyToAddress.ToString();
            }
            else
            {
                var replyQueue = replyToAddress.Queue;
                var @namespace = Determine(settings); //todo: inject
                return replyQueue + "@" + @namespace;
            }
        }

    }

}