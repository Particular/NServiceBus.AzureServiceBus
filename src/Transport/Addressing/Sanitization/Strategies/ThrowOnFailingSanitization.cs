namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailingSanitization : ISanitizationStrategy
    {
        ReadOnlySettings settings;
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex topicAndQueueNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");
        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex subscriptionAndRuleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public ThrowOnFailingSanitization(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            Validate(entityPathOrName, entityType);

            return entityPathOrName;
        }

        void Validate(string entityPath, EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Queue:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength))
                        ThrowException(entityPath, entityType, FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength)));

                    if (!topicAndQueueNameRegex.IsMatch(entityPath))
                        ThrowException(entityPath, entityType, FormatCharactersError(entityType, topicAndQueueNameRegex));
                    break;

                case EntityType.Topic:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength))
                        ThrowException(entityPath, entityType, FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength)));

                    if (!topicAndQueueNameRegex.IsMatch(entityPath))
                        ThrowException(entityPath, entityType, FormatCharactersError(entityType, topicAndQueueNameRegex));
                    break;

                case EntityType.Subscription:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength))
                        ThrowException(entityPath, entityType, FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength)));

                    if (!subscriptionAndRuleNameRegex.IsMatch(entityPath))
                        ThrowException(entityPath, entityType, FormatCharactersError(entityType, subscriptionAndRuleNameRegex));
                    break;

                case EntityType.Rule:
                    if (entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength))
                        ThrowException(entityPath, entityType, FormatLengthError(entityType, settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength)));

                    if (!subscriptionAndRuleNameRegex.IsMatch(entityPath))
                        ThrowException(entityPath, entityType, FormatCharactersError(entityType, subscriptionAndRuleNameRegex));
                    break;


                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }
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