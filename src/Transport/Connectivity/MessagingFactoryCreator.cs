namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceBus.Messaging.Amqp;
    using Settings;

    class MessagingFactoryCreator : ICreateMessagingFactoriesInternal
    {
        public MessagingFactoryCreator(IManageNamespaceManagerLifeCycleInternal namespaceManagers, ReadOnlySettings settings)
        {
            this.namespaceManagers = namespaceManagers;
            var transportType = settings.Get<TransportType>(WellKnownConfigurationKeys.Connectivity.TransportType);
            var batchFlushInterval = settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval);

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy))
            {
                retryPolicy = settings.Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy);
            }

            if (settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory))
            {
                settingsFactory = settings.Get<Func<string, MessagingFactorySettings>>(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory);
            }
            else
            {
                settingsFactory = namespaceName =>
                {
                    var factorySettings = new MessagingFactorySettings
                    {
                        TransportType = transportType
                    };

                    switch (transportType)
                    {
                        case TransportType.NetMessaging:
                            factorySettings.NetMessagingTransportSettings = new NetMessagingTransportSettings
                            {
                                BatchFlushInterval = batchFlushInterval
                            };
                            break;
                        case TransportType.Amqp:
                            factorySettings.AmqpTransportSettings = new AmqpTransportSettings
                            {
                                BatchFlushInterval = batchFlushInterval
                            };
                            break;
                    }

                    return factorySettings;
                };
            }
        }

        public IMessagingFactoryInternal Create(string namespaceName)
        {
            var namespaceManager = namespaceManagers.Get(namespaceName);
            var factorySettings = settingsFactory(namespaceName);

            // if none has been provided either by us or the customer we need to set one
            if (factorySettings.TokenProvider == null)
            {
                factorySettings.TokenProvider = namespaceManager.Settings.TokenProvider;
            }

            var inner = MessagingFactory.Create(namespaceManager.Address, factorySettings);

            if (retryPolicy != null)
            {
                inner.RetryPolicy = retryPolicy;
            }

            return new MessagingFactoryAdapter(inner);
        }

        IManageNamespaceManagerLifeCycleInternal namespaceManagers;
        Func<string, MessagingFactorySettings> settingsFactory;
        RetryPolicy retryPolicy;
    }
}