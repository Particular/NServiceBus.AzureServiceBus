namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Settings;
    using Topology.MetaModel;

    class NamespaceManagerCreator : ICreateNamespaceManagers
    {
        ReadOnlySettings settings;
        Func<string, NamespaceManagerSettings> settingsFactory;
        Func<string, TokenProvider> tokenProviderFactory;

        public NamespaceManagerCreator(ReadOnlySettings settings)
        {
            this.settings = settings;

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory))
            {
                settingsFactory = settings.Get<Func<string, NamespaceManagerSettings>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory);
            }
            else if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory))
            {
                tokenProviderFactory = settings.Get<Func<string, TokenProvider>>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory);
            }
        }

        public INamespaceManager Create(string @namespace)
        {
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var connectionString = @namespace;
            if (!ConnectionString.IsConnectionString(connectionString))
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
            else {
                if (tokenProviderFactory != null)
                {
                    var s = new NamespaceManagerSettings()
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

                if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy))
                {
                    manager.Settings.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy);
                }
            }
            return new NamespaceManagerAdapter(manager);
        }
    }
}