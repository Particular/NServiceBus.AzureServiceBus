namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ValidateAndHashIfNeeded : ISanitizationStrategy
    {
        readonly ReadOnlySettings settings;

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex queueAndTopicPathValidationRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");

        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex subscriptionAndRuleNameValidationRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        // Sanitize anything that is [NOT letters, numbers, periods (.), hyphens (-), underscores (-)], and leading or trailing slashes (/)
        Regex queueAndTopicPathSanitizationRegex = new Regex(@"[^a-zA-Z0-9\-\._/]|^/*|/*$");

        Regex subscriptionAndRuleNameSanitizationRegex = new Regex(@"[^a-zA-Z0-9\-\._]");

        Func<string, ValidationResult> defaultQueuePathValidation;
        Func<string, ValidationResult> defaultTopicPathValidation;
        Func<string, ValidationResult> defaultSubscriptionNameValidation;
        Func<string, ValidationResult> defaultRuleNameValidation;

        Func<string, string> defaultQueuePathSanitization;
        Func<string, string> defaultTopicPathSanitization;
        Func<string, string> defaultSubscriptionNameSanitization;
        Func<string, string> defaultRuleNameSanitization;

        Func<string, string> defaultHashing;

        public ValidateAndHashIfNeeded(ReadOnlySettings settings)
        {
            this.settings = settings;

            // validators

            defaultQueuePathValidation = queuePath =>
            {
                var validationResult = new ValidationResult();

                if (!queueAndTopicPathValidationRegex.IsMatch(queuePath))
                    validationResult.AddErrorForInvalidCharacters($"Queue path {queuePath} contains illegal characters. Legal characters should match the following regex: `{queuePath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
                if (queuePath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Queue path `{queuePath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultTopicPathValidation = topicPath =>
            {
                var validationResult = new ValidationResult();

                if (!queueAndTopicPathValidationRegex.IsMatch(topicPath))
                    validationResult.AddErrorForInvalidCharacters($"Topic path {topicPath} contains illegal characters. Legal characters should match the following regex: `{topicPath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
                if (topicPath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Topic path `{topicPath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultSubscriptionNameValidation = subscriptionName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameValidationRegex.IsMatch(subscriptionName))
                    validationResult.AddErrorForInvalidCharacters($"Subscription name {subscriptionName} contains illegal characters. Legal characters should match the following regex: `{subscriptionName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength);
                if (subscriptionName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Subscription name `{subscriptionName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultRuleNameValidation = ruleName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameValidationRegex.IsMatch(ruleName))
                    validationResult.AddErrorForInvalidCharacters($"Rule name {ruleName} contains illegal characters. Legal characters should match the following regex: `{ruleName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
                if (ruleName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Rule name `{ruleName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;

            };

            // sanitizers

            defaultQueuePathSanitization = queuePath => queueAndTopicPathSanitizationRegex.Replace(queuePath, string.Empty);
            defaultTopicPathSanitization = topicPath => queueAndTopicPathSanitizationRegex.Replace(topicPath, string.Empty);
            defaultSubscriptionNameSanitization = subscriptionPath => subscriptionAndRuleNameSanitizationRegex.Replace(subscriptionPath, string.Empty);
            defaultRuleNameSanitization = rulePath => subscriptionAndRuleNameSanitizationRegex.Replace(rulePath, string.Empty);

            // hash
            defaultHashing = entityPathOrName => MD5DeterministicNameBuilder.Build(entityPathOrName);
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            // remove characters invalid in v6
            Func<string, ValidationResult> validator;
            Func<string, string> sanitizer;

            switch (entityType)
            {
                case EntityType.Queue:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, out validator))
                    {
                        validator = defaultQueuePathValidation;
                    }
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, out sanitizer))
                    {
                        sanitizer = defaultQueuePathSanitization;
                    }
                    break;
                case EntityType.Topic:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, out validator))
                    {
                        validator = defaultTopicPathValidation;
                    }
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, out sanitizer))
                    {
                        sanitizer = defaultTopicPathSanitization;
                    }
                    break;

                case EntityType.Subscription:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, out validator))
                    {
                        validator = defaultSubscriptionNameValidation;
                    }
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, out sanitizer))
                    {
                        sanitizer = defaultSubscriptionNameSanitization;
                    }
                    break;

                case EntityType.Rule:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, out validator))
                    {
                        validator = defaultRuleNameValidation;
                    }
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, out sanitizer))
                    {
                        sanitizer = defaultRuleNameSanitization;
                    }
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

            Func<string, string> hash;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, out hash))
            {
                hash = defaultHashing;
            }

            return hash(sanitizedValue);
        }
    }
}