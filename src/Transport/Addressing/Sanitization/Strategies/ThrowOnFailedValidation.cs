namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailedValidation : ISanitizationStrategy
    {
        ReadOnlySettings settings;

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        static Regex queueAndTopicNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");

        // Except for subscriptions and rules, these cannot contain slashes (/)
        static Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public ThrowOnFailedValidation(ReadOnlySettings settings)
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
            
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            Func<string, ValidationResult> validator;

            switch (entityType)
            {
                case EntityType.Queue:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator);
                    break;

                case EntityType.Topic:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator);
                    break;

                case EntityType.Subscription:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator);
                    break;

                case EntityType.Rule:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator);
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