namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusMessageSenderSettings : ExposeSettings
    {
        SettingsHolder _settings;

        public AzureServiceBusMessageSenderSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Retry policy.
        /// <remarks>Default is RetryPolicy.Default</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings RetryPolicy(RetryPolicy retryPolicy)
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy, retryPolicy);

            return this;
        }

        /// <summary>
        /// Time period to wait until next attempt is made after operation is throttled.
        /// <remarks>Default is 10 seconds.</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings BackOffTimeOnThrottle(TimeSpan backoffTime)
        {
            Guard.AgainstNegative(nameof(backoffTime), backoffTime);
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, backoffTime);

            return this;
        }

        /// <summary>
        /// Number of retries when operation is throttled.
        /// <remarks>Default is 5 attempts.</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings RetryAttemptsOnThrottle(int count)
        {
            Guard.AgainstNegative(nameof(count), count);
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, count);

            return this;
        }

        /// <summary>
        /// Maximum message size allowed for sending.
        /// <remarks>Default is 256KB.</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings MaximuMessageSizeInKilobytes(int sizeInKilobytes)
        {
            Guard.AgainstNegativeAndZero(nameof(sizeInKilobytes), sizeInKilobytes);
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes, sizeInKilobytes);

            return this;
        }

        /// <summary>
        /// Message size padding percentage used for sending batched messages.
        /// <remarks>Default is 5%.</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings MessageSizePaddingPercentage(int percentage)
        {
            Guard.AgainstNegative(nameof(percentage), percentage);
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage, percentage);
            return this;
        }

        /// <summary>
        /// Behavior for oversized messages.
        /// <remarks>Default is throw an exception using <see cref="ThrowOnOversizedBrokeredMessages"/>.</remarks>
        /// </summary>
        public AzureServiceBusMessageSenderSettings OversizedBrokeredMessageHandler<T>(T instance) where T : IHandleOversizedBrokeredMessages
        {
            _settings.Set(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance, instance);

            return this;
        }
    }
}