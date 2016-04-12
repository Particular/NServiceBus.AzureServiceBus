namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using AzureServiceBus;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusSubscriptionSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusSubscriptionSettings(SettingsHolder settings)
           : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Customize subscription creation by providing <see cref="SubscriptionDescription"/>.
        /// </summary>
        public AzureServiceBusSubscriptionSettings DescriptionFactory(Func<string, string, ReadOnlySettings, SubscriptionDescription> factory)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DescriptionFactory, factory);

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings DefaultMessageTimeToLive(TimeSpan expiryTimespan)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive, expiryTimespan);

            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations, enableBatchedOperations);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnFilterEvaluationExceptions(bool enableDeadLetteringOnFilterEvaluationExceptions)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions, enableDeadLetteringOnFilterEvaluationExceptions);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration, enableDeadLetteringOnMessageExpiration);
            return this;
        }

        /// <summary>
        /// <remarks> Default is set not to forward.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings ForwardDeadLetteredMessagesTo(string forwardDeadLetteredMessagesTo)
        {
            return ForwardDeadLetteredMessagesTo(s => true, forwardDeadLetteredMessagesTo);
        }

        /// <summary>
        /// <remarks> Default is set not to forward.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, condition);
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, forwardDeadLetteredMessagesTo);

            return this;
        }


        /// <summary>
        /// <remarks> Default is 30 seconds.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings LockDuration(TimeSpan lockDuration)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration, lockDuration);
            return this;
        }

        /// <summary>
        /// <remarks> Default is 10.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount, maxDeliveryCount);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings RequiresSession(bool requiresSession)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.RequiresSession, requiresSession);
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle, autoDeleteOnIdle);
            return this;
        }
    }
}