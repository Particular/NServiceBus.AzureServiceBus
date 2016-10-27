namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;

    class MessagingFactoryCreator : ICreateMessagingFactories
    {
        IManageNamespaceManagerLifeCycle namespaceManagers;
        Func<string, MessagingFactorySettings> settingsFactory;
        ReadOnlySettings settings;

        public MessagingFactoryCreator(IManageNamespaceManagerLifeCycle namespaceManagers, ReadOnlySettings settings)
        {
            this.namespaceManagers = namespaceManagers;
            this.settings = settings;

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory))
            {
                settingsFactory = settings.Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory);
            }
            else
            {
                settingsFactory = namespaceName =>
                {
                    var namespaceManager = this.namespaceManagers.Get(namespaceName);

                    var transportType = this.settings.Get<TransportType>(WellKnownConfigurationKeys.Connectivity.TransportType);

                    var factorySettings = new MessagingFactorySettings
                    {
                        TokenProvider = namespaceManager.Settings.TokenProvider,
                        TransportType = transportType
                    };

                    var batchFlushInterval = this.settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval);

                    switch (transportType)
                    {
                        case TransportType.NetMessaging:
                            factorySettings.NetMessagingTransportSettings = new NetMessagingTransportSettings
                            {
                                BatchFlushInterval = batchFlushInterval,
                            };
                            break;
                        case TransportType.Amqp:
                            factorySettings.AmqpTransportSettings = new AmqpTransportSettings
                            {
                                BatchFlushInterval = batchFlushInterval,
                            };
                            break;
                    }

                    return factorySettings;
                };
            }
        }

        public IMessagingFactory Create(string namespaceName)
        {
            var namespaceManager = namespaceManagers.Get(namespaceName);
            var factorySettings = settingsFactory(namespaceName);
            var inner = MessagingFactory.Create(namespaceManager.Address, factorySettings);
            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy))
            {
                inner.RetryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy);
            }
            return new MessagingFactoryAdapter(inner);
        }

    }
}