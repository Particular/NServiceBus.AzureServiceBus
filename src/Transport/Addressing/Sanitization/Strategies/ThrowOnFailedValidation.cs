namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailedValidation : ISanitizationStrategy
    {
        ReadOnlySettings settings;

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        static Regex queueAndTopicNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$", RegexOptions.Compiled);

        // Except for subscriptions and rules, these cannot contain slashes (/)
        static Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$", RegexOptions.Compiled);

        Func<string, ValidationResult> defaultQueuePathValidation;
        Func<string, ValidationResult> defaultTopicPathValidation;
        Func<string, ValidationResult> defaultSubscriptionNameValidation;
        Func<string, ValidationResult> defaultRuleNameValidation;

        public ThrowOnFailedValidation(ReadOnlySettings settings)
        {
            this.settings = settings;

            // validators

            defaultQueuePathValidation = queuePath =>
            {
                var validationResult = new ValidationResult();

                if (!queueAndTopicNameRegex.IsMatch(queuePath))
                    validationResult.AddErrorForInvalidCharacters($"Queue path {queuePath} contains illegal characters. Legal characters should match the following regex: `{queuePath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
                if (queuePath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Queue path `{queuePath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultTopicPathValidation = topicPath =>
            {
                var validationResult = new ValidationResult();

                if (!queueAndTopicNameRegex.IsMatch(topicPath))
                    validationResult.AddErrorForInvalidCharacters($"Topic path {topicPath} contains illegal characters. Legal characters should match the following regex: `{topicPath}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
                if (topicPath.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Topic path `{topicPath}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultSubscriptionNameValidation = subscriptionName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameRegex.IsMatch(subscriptionName))
                    validationResult.AddErrorForInvalidCharacters($"Subscription name {subscriptionName} contains illegal characters. Legal characters should match the following regex: `{subscriptionName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength);
                if (subscriptionName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Subscription name `{subscriptionName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;
            };

            defaultRuleNameValidation = ruleName =>
            {
                var validationResult = new ValidationResult();

                if (!subscriptionAndRuleNameRegex.IsMatch(ruleName))
                    validationResult.AddErrorForInvalidCharacters($"Rule name {ruleName} contains illegal characters. Legal characters should match the following regex: `{ruleName}`.");

                var maximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
                if (ruleName.Length > maximumLength)
                    validationResult.AddErrorForInvalidLenth($"Rule name `{ruleName}` exceeds maximum length of {maximumLength} characters.");

                return validationResult;

            };
            
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            Func<string, ValidationResult> validator;

            switch (entityType)
            {
                case EntityType.Queue:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, out validator))
                    {
                        validator = defaultQueuePathValidation;
                    }
                    break;

                case EntityType.Topic:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, out validator))
                    {
                        validator = defaultTopicPathValidation;
                    }
                    break;

                case EntityType.Subscription:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, out validator))
                    {
                        validator = defaultSubscriptionNameValidation;
                    }
                    break;

                case EntityType.Rule:
                    if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, out validator))
                    {
                        validator = defaultRuleNameValidation;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            var validationResult = validator(entityPathOrName);
            if (!validationResult.CharactersAreValid)
                ThrowException(entityPathOrName, entityType, validationResult.CharactersError);

            if (!validationResult.LengthIsValid)
                ThrowException(entityPathOrName, entityType, validationResult.LengthError);

            return entityPathOrName;
        }

        static void ThrowException(string entityPathOrName, EntityType entityType, string error)
        {

            var pathOrName = "path";

            if (entityType == EntityType.Rule || entityType == EntityType.Subscription)
            {
                pathOrName = "name";
            }

            throw new Exception($"Invalid {entityType} entity {pathOrName} `{entityPathOrName}` that cannot be used with Azure Service Bus. {error}"
                                + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
        }
    }
}