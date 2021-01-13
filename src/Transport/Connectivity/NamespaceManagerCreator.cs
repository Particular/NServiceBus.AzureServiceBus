namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Settings;

    class NamespaceManagerCreator : ICreateNamespaceManagersInternal
    {
        public NamespaceManagerCreator(ReadOnlySettings settings)
        {
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory))
            {
                settingsFactory = settings.Get<Func<string, NamespaceManagerSettings>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory);
            }
            else if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory))
            {
                tokenProviderFactory = settings.Get<Func<string, TokenProvider>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory);
            }

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy))
            {
                retryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy);
            }

            namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
        }

        public INamespaceManagerInternal Create(string @namespace)
        {

            var connectionString = @namespace;
            if (!ConnectionStringInternal.IsConnectionString(connectionString))
            {
                connectionString = namespacesDefinition.GetConnectionString(connectionString);
            }

            NamespaceManager manager;
            if (settingsFactory != null)
            {
                var s = settingsFactory(connectionString);
                var builder = new ServiceBusConnectionStringBuilder(connectionString);
                manager = new NamespaceManager(builder.GetAbsoluteRuntimeEndpoints(), s);
            }
            else
            {
                if (tokenProviderFactory != null)
                {
                    var s = new NamespaceManagerSettings
                    {
                        TokenProvider = tokenProviderFactory(connectionString)
                    };
                    var builder = new ServiceBusConnectionStringBuilder(connectionString);
                    manager = new NamespaceManager(builder.GetAbsoluteRuntimeEndpoints(), s);
                }
                else
                {
                    manager = NamespaceManager.CreateFromConnectionString(connectionString);
                }

                if (retryPolicy != null)
                {
                    manager.Settings.RetryPolicy = retryPolicy;
                }
            }
            return new NamespaceManagerAdapterInternal(manager);
        }

        Func<string, NamespaceManagerSettings> settingsFactory;
        Func<string, TokenProvider> tokenProviderFactory;
        RetryPolicy retryPolicy;
        NamespaceConfigurations namespacesDefinition;
    }
}