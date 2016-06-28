namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ValidateAndHashIfNeeded : ISanitizationStrategy
    {
        readonly ReadOnlySettings settings;

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex queueAndTopicNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");

        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        Regex sanitizationRegex = new Regex(@"[^a-zA-Z0-9\-\._]");

        public ValidateAndHashIfNeeded(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public void SetDefaultRules(SettingsHolder settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength, 260);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength, 50);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength, 50);

            // validators

            Func<string, ValidationResult> qpv = queuePath =>
            {
                var validationResult = new ValidationResult();

                if (!queueAndTopicNameRegex.IsMatch(queuePath))
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

                if (!queueAndTopicNameRegex.IsMatch(topicPath))
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

            Func<string, string> qps = queuePath => sanitizationRegex.Replace(queuePath, string.Empty);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, qps);

            Func<string, string> tps = topicPath => sanitizationRegex.Replace(topicPath, string.Empty);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, tps);

            Func<string, string> sns = subscriptionPath => sanitizationRegex.Replace(subscriptionPath, string.Empty);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, sns);

            Func<string, string> rns = rulePath => sanitizationRegex.Replace(rulePath, string.Empty);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, rns);

            // hash

            Func<string, string> hash = entityPathOrName => MD5DeterministicNameBuilder.Build(entityPathOrName);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, hash);
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            // remove characters invalid in v6
            Func<string, ValidationResult> validator;
            Func<string, string> sanitizer;

            switch (entityType)
            {
                case EntityType.Queue:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator);
                    sanitizer = settings.GetOrDefault<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer);
                    break;

                case EntityType.Topic:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator);
                    sanitizer = settings.GetOrDefault<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer);
                    break;

                case EntityType.Subscription:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator);
                    sanitizer = settings.GetOrDefault<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer);
                    break;

                case EntityType.Rule:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator);
                    sanitizer = settings.GetOrDefault<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            var validationResult = validator(entityPathOrName);
            if (validationResult.IsValid)
                return entityPathOrName;

            var sanitizedValue = entityPathOrName;
            if (!validationResult.CharactersAreValid)
                sanitizedValue = sanitizer(entityPathOrName);

            // second validation pass to validate length based on sanitized characters
            validationResult = validator(sanitizedValue);
            if (validationResult.LengthIsValid)
                return sanitizedValue;

            return settings.GetOrDefault<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash)(sanitizedValue);
        }
    }
}