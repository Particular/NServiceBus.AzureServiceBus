namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class EntityNameValidationV6Rules : IValidationStrategy
    {
        ReadOnlySettings settings;
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        // Except for subscriptions, these cannot contain slashes (/)
        Regex v6PathRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public EntityNameValidationV6Rules(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public ValidationResult IsValid(string entityPath, EntityType entityType)
        {
            var validationResult = new ValidationResult();

            switch (entityType)
            {
                case EntityType.Queue:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength)));
                    break;

                case EntityType.Topic:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength)));
                    break;

                case EntityType.Subscription:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength)));
                    break;

                case EntityType.Rule:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength)));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            if (!v6PathRegex.IsMatch(entityPath))
                validationResult.AddErrorForInvalidCharacter(FormatCharactersError(entityType, v6PathRegex));

            return validationResult;
        }

        static string FormatLengthError(EntityType entityType, int maximumLength)
        {
            var pathOrName = entityType == EntityType.Queue || entityType == EntityType.Topic ? "path" : "topic";
            return $"{entityType} {pathOrName} exceeds maximum length of {maximumLength} characters.";
        }
        static string FormatCharactersError(EntityType entityType, Regex regex)
        {
            var pathOrName = entityType == EntityType.Queue || entityType == EntityType.Topic ? "path" : "topic";
            return $"{entityType} {pathOrName} contains illegal characters. Legal characters should match the following regex: `{regex}`.";
        }
    }
}