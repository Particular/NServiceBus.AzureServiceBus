namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class EntityNameValidationRules : IValidationStrategy
    {
        ReadOnlySettings settings;
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex topicAndQueueNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");
        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public EntityNameValidationRules(ReadOnlySettings settings)
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

                    if (!topicAndQueueNameRegex.IsMatch(entityPath))
                        validationResult.AddErrorForInvalidCharacter(FormatCharactersError(entityType, topicAndQueueNameRegex));
                    break;

                case EntityType.Topic:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength)));

                    if (!topicAndQueueNameRegex.IsMatch(entityPath))
                        validationResult.AddErrorForInvalidCharacter(FormatCharactersError(entityType, topicAndQueueNameRegex));
                    break;

                case EntityType.Subscription:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength)));

                    if (!subscriptionAndRuleNameRegex.IsMatch(entityPath))
                        validationResult.AddErrorForInvalidCharacter(FormatCharactersError(entityType, subscriptionAndRuleNameRegex));
                    break;

                case EntityType.Rule:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength))
                        validationResult.AddErrorForInvalidLength(FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength)));

                    if (!subscriptionAndRuleNameRegex.IsMatch(entityPath))
                        validationResult.AddErrorForInvalidCharacter(FormatCharactersError(entityType, subscriptionAndRuleNameRegex));
                    break;


                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

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