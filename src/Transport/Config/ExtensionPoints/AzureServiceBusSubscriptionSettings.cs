namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public partial class AzureServiceBusSubscriptionSettings : ExposeSettings
    {
        TopologySubscriptionSettings subscriptionSettings;

        internal AzureServiceBusSubscriptionSettings(SettingsHolder settings) : base(settings)
        {
            subscriptionSettings = settings.Get<ITopologyInternal>().Settings.SubscriptionSettings;
        }

        /// <summary>
        /// Customize subscription creation by providing <see cref="SubscriptionDescription"/>.
        /// </summary>
        public AzureServiceBusSubscriptionSettings DescriptionFactory(Action<SubscriptionDescription> factory)
        {
            subscriptionSettings.DescriptionFactory = factory;

            return this;
        }

        /// <summary>
        /// <remarks> Default is TimeSpan.MaxValue.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings DefaultMessageTimeToLive(TimeSpan expiryTimespan)
        {
            subscriptionSettings.DefaultMessageTimeToLive = expiryTimespan;

            return this;
        }

        /// <summary>
        /// <remarks> Default is true.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableBatchedOperations(bool enableBatchedOperations)
        {
            subscriptionSettings.EnableBatchedOperations = enableBatchedOperations;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnFilterEvaluationExceptions(bool enableDeadLetteringOnFilterEvaluationExceptions)
        {
            subscriptionSettings.EnableDeadLetteringOnFilterEvaluationExceptions = enableDeadLetteringOnFilterEvaluationExceptions;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            subscriptionSettings.EnableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
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
        // TODO: needs to be deprecated
        public AzureServiceBusSubscriptionSettings ForwardDeadLetteredMessagesTo(Func<string, bool> condition, string forwardDeadLetteredMessagesTo)
        {
            subscriptionSettings.ForwardDeadLetteredMessagesToCondition = condition;
            subscriptionSettings.ForwardDeadLetteredMessagesTo = forwardDeadLetteredMessagesTo;

            return this;
        }


        /// <summary>
        /// <remarks> Default is 30 seconds.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings LockDuration(TimeSpan lockDuration)
        {
            subscriptionSettings.LockDuration = lockDuration;
            return this;
        }

        /// <summary>
        /// <remarks> Default is 10.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings MaxDeliveryCount(int maxDeliveryCount)
        {
            subscriptionSettings.MaxDeliveryCount = maxDeliveryCount;
            return this;
        }

        /// <summary>
        /// <remarks> Default is false.</remarks>
        /// </summary>
        public AzureServiceBusSubscriptionSettings AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            subscriptionSettings.AutoDeleteOnIdle = autoDeleteOnIdle;
            return this;
        }
    }
}