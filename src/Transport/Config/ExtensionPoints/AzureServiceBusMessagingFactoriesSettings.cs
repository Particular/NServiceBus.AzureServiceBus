namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusMessagingFactoriesSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusMessagingFactoriesSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Customize <see cref="MessagingFactory"/> creation.
        /// </summary>
        public AzureServiceBusMessagingFactoriesSettings MessagingFactorySettingsFactory(Func<string, MessagingFactorySettings> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.MessagingFactorySettingsFactory, factory);

            return this;
        }

        /// <summary>
        /// Number of messaging factories per namespace to create senders and receivers.
        /// <remarks>Default is 5.</remarks>
        /// </summary>
        public AzureServiceBusMessagingFactoriesSettings NumberOfMessagingFactoriesPerNamespace(int number)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, number);

            return this;
        }

        /// <summary>
        /// Retry policy configured on MessagingFactory level.
        /// <remarks>Default is RetryPolicy.Default</remarks>
        /// </summary>
        public AzureServiceBusMessagingFactoriesSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.RetryPolicy, retryPolicy);

            return this;
        }

        /// <summary>
        /// Batch flush interval configured on MessagingFactory level.
        /// <remarks>Default is 0.5 seconds.</remarks>
        /// </summary>
        public AzureServiceBusMessagingFactoriesSettings BatchFlushInterval(TimeSpan batchFlushInterval)
        {
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval, batchFlushInterval);

            return this;
        }
    }
}