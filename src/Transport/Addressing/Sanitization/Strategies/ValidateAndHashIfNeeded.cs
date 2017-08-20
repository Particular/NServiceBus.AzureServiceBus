namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;
    using Transport.AzureServiceBus;

    public class ValidateAndHashIfNeeded : ISanitizationStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
            var maximumQueuePathLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, out queuePathValidation))
            {
                queuePathValidation = queuePath =>
                {
                    ValidationResult validationResult = null;

                    if (!queueAndTopicPathValidationRegex.IsMatch(queuePath))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Queue path {queuePath} contains illegal characters. Legal characters should match the following regex: `{queuePath}`.");
                    }
                
                    if (queuePath.Length > maximumQueuePathLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Queue path `{queuePath}` exceeds maximum length of {maximumQueuePathLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathSanitizer, out queuePathSanitization))
            {
                queuePathSanitization = queuePath => queueAndTopicPathSanitizationRegex.Replace(queuePath, string.Empty);
            }

            var topicPathMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, out topicPathValidation))
            {
                topicPathValidation = topicPath =>
                {
                    ValidationResult validationResult = null;

                    if (!queueAndTopicPathValidationRegex.IsMatch(topicPath))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Topic path {topicPath} contains illegal characters. Legal characters should match the following regex: `{topicPath}`.");
                    }

                
                    if (topicPath.Length > topicPathMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Topic path `{topicPath}` exceeds maximum length of {topicPathMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathSanitizer, out topicPathSanitization))
            {
                topicPathSanitization = topicPath => queueAndTopicPathSanitizationRegex.Replace(topicPath, string.Empty);
            }

            var subscriptionNameMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, out subscriptionNameValidation))
            {
                subscriptionNameValidation = subscriptionName =>
                {
                    ValidationResult validationResult = null;

                    if (!subscriptionAndRuleNameValidationRegex.IsMatch(subscriptionName))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Subscription name {subscriptionName} contains illegal characters. Legal characters should match the following regex: `{subscriptionName}`.");
                    }

                
                    if (subscriptionName.Length > subscriptionNameMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Subscription name `{subscriptionName}` exceeds maximum length of {subscriptionNameMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameSanitizer, out subscriptionNameSanitization))
            {
                subscriptionNameSanitization = subscriptionPath => subscriptionAndRuleNameSanitizationRegex.Replace(subscriptionPath, string.Empty);
            }

            var ruleNameMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, out ruleNameValidation))
            {
                ruleNameValidation = ruleName =>
                {
                    ValidationResult validationResult = null;

                    if (!subscriptionAndRuleNameValidationRegex.IsMatch(ruleName))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Rule name {ruleName} contains illegal characters. Legal characters should match the following regex: `{ruleName}`.");
                    }
                
                    if (ruleName.Length > ruleNameMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Rule name `{ruleName}` exceeds maximum length of {ruleNameMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameSanitizer, out ruleNameSanitization))
            {
                ruleNameSanitization = rulePath => subscriptionAndRuleNameSanitizationRegex.Replace(rulePath, string.Empty);
            }

            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, out hashing))
            {
                hashing = entityPathOrName => MD5DeterministicNameBuilder.Build(entityPathOrName);
            }
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            // remove characters invalid in v6
            Func<string, ValidationResult> validator;
            Func<string, string> sanitizer;

            switch (entityType)
            {
                case EntityType.Queue:
                    validator = queuePathValidation;
                    sanitizer = queuePathSanitization;
                    break;
                case EntityType.Topic:
                    validator = topicPathValidation;
                    sanitizer = topicPathSanitization;
                    break;
                case EntityType.Subscription:
                    validator = subscriptionNameValidation;
                    sanitizer = subscriptionNameSanitization;
                    break;
                case EntityType.Rule:
                    validator = ruleNameValidation;
                    sanitizer = ruleNameSanitization;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            var validationResult = validator(entityPathOrName);
            if (validationResult.IsValid)
            {
                return entityPathOrName;
            }

            var sanitizedValue = entityPathOrName;
            if (!validationResult.CharactersAreValid)
            {
                sanitizedValue = sanitizer(entityPathOrName);
            }

            // second validation pass to validate length based on sanitized characters
            validationResult = validator(sanitizedValue);
            if (validationResult.LengthIsValid)
            {
                return sanitizedValue;
            }

            return hashing(sanitizedValue);
        }

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        internal static Regex queueAndTopicPathValidationRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$", RegexOptions.Compiled);

        // Except for subscriptions and rules, these cannot contain slashes (/)
        internal static Regex subscriptionAndRuleNameValidationRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$", RegexOptions.Compiled);

        // Sanitize anything that is [NOT letters, numbers, periods (.), hyphens (-), underscores (-)], and leading or trailing slashes (/)
        static Regex queueAndTopicPathSanitizationRegex = new Regex(@"[^a-zA-Z0-9\-\._/]|^/*|/*$", RegexOptions.Compiled);

        static Regex subscriptionAndRuleNameSanitizationRegex = new Regex(@"[^a-zA-Z0-9\-\._]", RegexOptions.Compiled);

        Func<string, ValidationResult> queuePathValidation;
        Func<string, ValidationResult> topicPathValidation;
        Func<string, ValidationResult> subscriptionNameValidation;
        Func<string, ValidationResult> ruleNameValidation;

        Func<string, string> queuePathSanitization;
        Func<string, string> topicPathSanitization;
        Func<string, string> subscriptionNameSanitization;
        Func<string, string> ruleNameSanitization;

        Func<string, string> hashing;
    }
}