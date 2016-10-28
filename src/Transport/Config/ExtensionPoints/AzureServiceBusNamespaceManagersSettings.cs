namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Microsoft.ServiceBus;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusNamespaceManagersSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusNamespaceManagersSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Customize <see cref="NamespaceManager"/> creation.
        /// </summary>
        public AzureServiceBusNamespaceManagersSettings NamespaceManagerSettingsFactory(Func<string, NamespaceManagerSettings> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.NamespaceManagerSettingsFactory, factory);

            return this;
        }

        /// <summary>
        /// Customize the token provider.
        /// </summary>
        public AzureServiceBusNamespaceManagersSettings TokenProvider(Func<string, TokenProvider> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.TokenProviderFactory, factory);

            return this;
        }

        /// <summary>
        /// Retry policy configured on Namespace Manager level.
        /// <remarks>Default is RetryPolicy.Default</remarks>
        /// </summary>
        public AzureServiceBusNamespaceManagersSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.NamespaceManagers.RetryPolicy, retryPolicy);

            return this;
        }
        
    }
}