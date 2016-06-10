namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class EndpointOrientedTopologySanitization : ISanitizationStrategy
    {
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        // Except for subscriptions, these cannot contain slashes (/)
        Regex v6PathRegex = new Regex(@"[^a-zA-Z0-9\-\._]");
        readonly ReadOnlySettings settings;

        public EndpointOrientedTopologySanitization(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            // remove characters invalid in v6
            entityPathOrName = v6PathRegex.Replace(entityPathOrName, "");

            return ValidateAndSanitizeLengthOfEntityPathOrName(entityPathOrName, entityType);
        }
        
        string ValidateAndSanitizeLengthOfEntityPathOrName(string entityPath, EntityType entityType)
        {
            bool shouldHash;

            switch (entityType)
            {
                case EntityType.Queue:
                    shouldHash = entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength);
                    break;

                case EntityType.Topic:
                    shouldHash = entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength);
                    break;

                case EntityType.Subscription:
                    shouldHash = entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength);
                    break;

                case EntityType.Rule:
                    shouldHash = entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            if (shouldHash)
            {
                return MD5DeterministicNameBuilder.Build(entityPath);
            }
            return entityPath;
        }
    }
}