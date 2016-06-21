namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using Settings;

    public class ValidateAndHashIfNeeded : ISanitizationStrategy
    {
        readonly ReadOnlySettings settings;

        public ValidateAndHashIfNeeded(ReadOnlySettings settings)
        {
            this.settings = settings;
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