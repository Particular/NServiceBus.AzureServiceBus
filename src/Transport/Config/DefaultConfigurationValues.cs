namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Addressing;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Topology.MetaModel;
    using Settings;

    class DefaultConfigurationValues
    {
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        static Regex regex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");

        // Except for subscriptions and rules, these cannot contain slashes (/)
        static Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public SettingsHolder Apply(SettingsHolder settings)
        {
            ApplyDefaultsForConnectivity(settings);
            ApplyDefaultValuesForAddressing(settings);
            ApplyDefaultValuesForQueueDescriptions(settings);
            ApplyDefaultValuesForTopics(settings);
            ApplyDefaultValuesForSubscriptions(settings);
            ApplyDefaultValuesForSerialization(settings);
            ApplyDefaultValuesForSanitization(settings);

            return settings;
        }

        void ApplyDefaultValuesForSanitization(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, 50);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, 50);

            // validators

            Func<string, ValidationResult> qpv = queuePath =>
            {
                var validationResult = new ValidationResult();

                if (!regex.IsMatch(queuePath))
                    validationResult.AddErrorForInvalidCharacters($"Queue path {queuePath} contains illegal characters. Legal characters should match the following regex: `{queuePath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
                if (queuePath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Queue path `{queuePath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, qpv);

            Func<string, ValidationResult> tpv = topicPath =>
            {
                var validationResult = new ValidationResult();

                if (!regex.IsMatch(topicPath))
                    validationResult.AddErrorForInvalidCharacters($"Topic path {topicPath} contains illegal characters. Legal characters should match the following regex: `{topicPath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
                if (topicPath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Topic path `{topicPath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, tpv);

            Func<string, ValidationResult> spv = subscriptionName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameRegex.IsMatch(subscriptionName))
                    validationResult.AddErrorForInvalidCharacters($"Subscription name {subscriptionName} contains illegal characters. Legal characters should match the following regex: `{subscriptionName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength);
                if (subscriptionName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Subscription name `{subscriptionName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, spv);

            Func<string, ValidationResult> rpv = ruleName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameRegex.IsMatch(ruleName))
                    validationResult.AddErrorForInvalidCharacters($"Rule name {ruleName} contains illegal characters. Legal characters should match the following regex: `{ruleName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
                if (ruleName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Rule name `{ruleName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;

            };
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, rpv);

            // sanitizers

            Func<string, string> qps = queuePath => queuePath;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, qps);

            Func<string, string> tps = topicPath => topicPath;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, tps);

            Func<string, string> sns = subscriptionPath => subscriptionPath;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, sns);

            Func<string, string> rns = rulePath => rulePath;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, rns);

            // hash

            Func<string, string> hash = entityPathOrName => entityPathOrName;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, hash);
        }

        void ApplyDefaultValuesForSerialization(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.ByteArray);
        }

        void ApplyDefaultValuesForAddressing(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, new NamespaceConfigurations());
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings, typeof(DefaultNamespaceNameToConnectionStringMapper));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceName, new Func<string>(() => "default"));
        }

        void ApplyDefaultsForConnectivity(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, true);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.ConnectivityMode, ConnectivityMode.Tcp);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.NumberOfMessagingFactoriesPerNamespace, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, TimeSpan.FromMinutes(5.0));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 200);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessagingFactories.BatchFlushInterval, TimeSpan.FromSeconds(0.5));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle, TimeSpan.FromSeconds(10));
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes, 256);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage, 5);
            settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance, new ThrowOnOversizedBrokeredMessages());
        }

        void ApplyDefaultValuesForQueueDescriptions(SettingsHolder settings)
        {
            settings.Set("Transport.CreateQueues", true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes, (long) 1024);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));

            var maxDeliveryCount = 10;// (!settings.HasExplicitValue(typeof(FirstLevelRetries).FullName) || settings.IsFeatureEnabled(typeof(FirstLevelRetries))) ? 6 : 1;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount, maxDeliveryCount);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations, true);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpressCondition, new Func<string, bool>(name => true));

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo, null);
        }

        void ApplyDefaultValuesForTopics(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(600000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableBatchedOperations, true);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpress, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableExpressCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnableFilteringMessagesBeforePublishing, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.MaxSizeInMegabytes, (long)1024);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.RequiresDuplicateDetection, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Topics.SupportOrdering, false);
        }

        void ApplyDefaultValuesForSubscriptions(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.AutoDeleteOnIdle, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.DefaultMessageTimeToLive, TimeSpan.MaxValue);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.LockDuration, TimeSpan.FromMilliseconds(30000));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableBatchedOperations, true);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnFilterEvaluationExceptions, false);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.EnableDeadLetteringOnMessageExpiration, false);
            var maxDeliveryCount = 10;// (!settings.HasExplicitValue(typeof(FirstLevelRetries).FullName) || settings.IsFeatureEnabled(typeof(FirstLevelRetries))) ? 6 : 1;
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.MaxDeliveryCount, maxDeliveryCount);

            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesToCondition, new Func<string, bool>(name => true));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Resources.Subscriptions.ForwardDeadLetteredMessagesTo, null);
        }
    }
}