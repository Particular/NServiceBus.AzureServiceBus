namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class MessagingFactoryCreator : ICreateMessagingFactories
    {
        readonly IManageNamespaceManagerLifeCycle _namespaceManagers;
        Func<string, MessagingFactorySettings> _settingsFactory;
        readonly ReadOnlySettings _settings;

        public MessagingFactoryCreator(IManageNamespaceManagerLifeCycle namespaceManagers, ReadOnlySettings settings)
        {
            this._namespaceManagers = namespaceManagers;
            this._settings = settings;

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory))
            {
                _settingsFactory = settings.Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory);
            }
            else
            {
                _settingsFactory = connectionstring =>
                {
                    var namespaceManager = _namespaceManagers.Get(connectionstring);

                    var s = new MessagingFactorySettings
                    {
                        TokenProvider = namespaceManager.Settings.TokenProvider,
                        NetMessagingTransportSettings =
                        {
                            BatchFlushInterval = TimeSpan.FromSeconds(0.1)
                        }
                    };

                    return s;
                };
            }
        }

        public IMessagingFactory Create(string connectionstring)
        {
            var namespaceManager = _namespaceManagers.Get(connectionstring);
            var factorySettings = _settingsFactory(connectionstring);
            var inner = MessagingFactory.Create(namespaceManager.Address, factorySettings);
            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.PrefetchCount))
            {
                inner.PrefetchCount = _settings.Get<int>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.PrefetchCount);
            }
            if (_settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy))
            {
                inner.RetryPolicy = _settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy);
            }
            return new MessagingFactoryAdapter(inner);
        }
        
    }
}