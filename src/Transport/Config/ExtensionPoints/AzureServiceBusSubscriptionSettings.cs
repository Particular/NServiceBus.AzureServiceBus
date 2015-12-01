namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSubscriptionSettings : ExposeSettings
    {
         SettingsHolder _settings;

         public AzureServiceBusSubscriptionSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

         public AzureServiceBusSubscriptionSettings DescriptionFactory(Func<string, string, ReadOnlySettings, SubscriptionDescription> factory)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DescriptionFactory, factory);

            return this;
        }

        public AzureServiceBusSubscriptionSettings DefaultMessageTimeToLive(TimeSpan expiryTimespan)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive, expiryTimespan);

            return this;
        }

        public AzureServiceBusSubscriptionSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations, enableBatchedOperations);
            return this;
        }

        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnFilterEvaluationExceptions(bool enableDeadLetteringOnFilterEvaluationExceptions)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions, enableDeadLetteringOnFilterEvaluationExceptions);
            return this;
        }

        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration, enableDeadLetteringOnMessageExpiration);
            return this;
        }

        public AzureServiceBusSubscriptionSettings ForwardDeadLetteredMessagesTo(string forwardDeadLetteredMessagesTo)
        {
            return ForwardDeadLetteredMessagesTo(s => true, forwardDeadLetteredMessagesTo);
        }
        public AzureServiceBusSubscriptionSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, condition);
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, forwardDeadLetteredMessagesTo);

            return this;
        }

        public AzureServiceBusSubscriptionSettings ForwardTo(string forwardTo)
        {
            return ForwardTo(n => true, forwardTo);
        }
        public AzureServiceBusSubscriptionSettings ForwardTo(Func<string, bool> condition, string forwardTo)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardTo, forwardTo);
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardToCondition, condition);

            return this;
        }

        public AzureServiceBusSubscriptionSettings LockDuration(TimeSpan lockDuration)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration, lockDuration);
            return this;
        }

        public AzureServiceBusSubscriptionSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount, maxDeliveryCount);
            return this;
        }

        public AzureServiceBusSubscriptionSettings RequiresSession(bool requiresSession)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession, requiresSession);
            return this;
        }

        public AzureServiceBusSubscriptionSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            _settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle, autoDeleteOnIdle);
            return this;
        }
    }
}