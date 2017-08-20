namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    public class ThrowOnFailedValidation : ISanitizationStrategy
    {
        internal ThrowOnFailedValidation(ReadOnlySettings settings)
        {
            var maximumQueuePathLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator, out queuePathValidation))
            {
                queuePathValidation = queuePath =>
                {
                    ValidationResult validationResult = null;

                    if (!ValidateAndHashIfNeeded.queueAndTopicPathValidationRegex.IsMatch(queuePath))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Queue path {queuePath} contains illegal characters. Legal characters should match the following regex: `{ValidateAndHashIfNeeded.queueAndTopicPathValidationRegex}`.");
                    }

                    if (queuePath.Length > maximumQueuePathLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Queue path `{queuePath}` exceeds maximum length of {maximumQueuePathLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }

            var topicPathMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator, out topicPathValidation))
            {
                topicPathValidation = topicPath =>
                {
                    ValidationResult validationResult = null;

                    if (!ValidateAndHashIfNeeded.queueAndTopicPathValidationRegex.IsMatch(topicPath))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Topic path {topicPath} contains illegal characters. Legal characters should match the following regex: `{ValidateAndHashIfNeeded.queueAndTopicPathValidationRegex}`.");
                    }


                    if (topicPath.Length > topicPathMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Topic path `{topicPath}` exceeds maximum length of {topicPathMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }

            var subscriptionNameMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator, out subscriptionNameValidation))
            {
                subscriptionNameValidation = subscriptionName =>
                {
                    ValidationResult validationResult = null;

                    if (!ValidateAndHashIfNeeded.subscriptionAndRuleNameValidationRegex.IsMatch(subscriptionName))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Subscription name {subscriptionName} contains illegal characters. Legal characters should match the following regex: `{ValidateAndHashIfNeeded.subscriptionAndRuleNameValidationRegex}`.");
                    }


                    if (subscriptionName.Length > subscriptionNameMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Subscription name `{subscriptionName}` exceeds maximum length of {subscriptionNameMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }

            var ruleNameMaximumLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator, out ruleNameValidation))
            {
                ruleNameValidation = ruleName =>
                {
                    ValidationResult validationResult = null;

                    if (!ValidateAndHashIfNeeded.subscriptionAndRuleNameValidationRegex.IsMatch(ruleName))
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddErrorForInvalidCharacters($"Rule name {ruleName} contains illegal characters. Legal characters should match the following regex: `{ValidateAndHashIfNeeded.subscriptionAndRuleNameValidationRegex}`.");
                    }

                    if (ruleName.Length > ruleNameMaximumLength)
                    {
                        validationResult = validationResult ?? new ValidationResult();
                        validationResult.AddErrorForInvalidLenth($"Rule name `{ruleName}` exceeds maximum length of {ruleNameMaximumLength} characters.");
                    }

                    return validationResult ?? ValidationResult.Empty;
                };
            }
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            Func<string, ValidationResult> validator;

            switch (entityType)
            {
                case EntityType.Queue:
                    validator = queuePathValidation;
                    break;
                case EntityType.Topic:
                    validator = topicPathValidation;
                    break;
                case EntityType.Subscription:
                    validator = subscriptionNameValidation;
                    break;
                case EntityType.Rule:
                    validator = ruleNameValidation;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            var validationResult = validator(entityPathOrName);
            if (!validationResult.CharactersAreValid)
            {
                ThrowException(entityPathOrName, entityType, validationResult.CharactersError);
            }

            if (!validationResult.LengthIsValid)
            {
                ThrowException(entityPathOrName, entityType, validationResult.LengthError);
            }

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

        Func<string, ValidationResult> queuePathValidation;
        Func<string, ValidationResult> topicPathValidation;
        Func<string, ValidationResult> subscriptionNameValidation;
        Func<string, ValidationResult> ruleNameValidation;
    }
}