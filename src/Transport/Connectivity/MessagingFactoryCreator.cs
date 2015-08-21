namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;

    class MessagingFactoryCreator : ICreateMessagingFactories
    {
        readonly NamespaceManagerLifeCycleManager _namespaceManagers;
        Func<string, MessagingFactorySettings> _settingsFactory;

        public MessagingFactoryCreator(NamespaceManagerLifeCycleManager namespaceManagers, ReadOnlySettings settings)
        {
            this._namespaceManagers = namespaceManagers;

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactorySettingsFactory))
            {
                _settingsFactory = settings.Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactorySettingsFactory);
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
            var settings = _settingsFactory(connectionstring);
            return new MessagingFactoryAdapter(MessagingFactory.Create(connectionstring, settings));
        }

        
    }
}