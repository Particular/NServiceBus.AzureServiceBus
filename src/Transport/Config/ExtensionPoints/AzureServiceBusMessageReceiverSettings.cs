namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusMessageReceiverSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusMessageReceiverSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Brokered messages receive mode.
        /// <remarks>Default is ReceiveMode.PeekLock.</remarks>
        /// </summary>
        public AzureServiceBusMessageReceiverSettings ReceiveMode(ReceiveMode receiveMode)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, receiveMode);

            return this;
        }

        /// <summary>
        /// Number of messages to pre-fetch in receive operation.
        /// </summary>
        public AzureServiceBusMessageReceiverSettings PrefetchCount(int prefetchCount)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, prefetchCount);

            return this;
        }

        /// <summary>
        /// Retry policy.
        /// <remarks>Default is RetryPolicy.Default</remarks>
        /// </summary>
        public AzureServiceBusMessageReceiverSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.RetryPolicy, retryPolicy);

            return this;
        }

        /// <summary>
        /// Maximum duration within which the message lock will be renewed automatically. 
        /// <remarks>This value should be greater than the message lock duration. Default is 5 minutes.</remarks>
        /// </summary>
        public AzureServiceBusMessageReceiverSettings AutoRenewTimeout(TimeSpan autoRenewTimeout)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, autoRenewTimeout);

            return this;
        }
    }
}