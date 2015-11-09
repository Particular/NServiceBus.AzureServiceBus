namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using NServiceBus.Settings;

    public class EntityNameValidationRules : IValidationStrategy
    {
        readonly ReadOnlySettings settings;

        public EntityNameValidationRules(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public bool IsValid(string entityPath, EntityType entityType)
        {
            // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
            var topicQueueNameRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");
            // Except for subscriptions, these cannot contain slashes (/)
            var subscriptionNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");
            var valid = true;

            switch (entityType)
            {
                case EntityType.Queue:
                    valid &= entityPath.Length <= settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength);
                    valid &= topicQueueNameRegex.IsMatch(entityPath);
                    break;
                case EntityType.Topic:
                    valid &= entityPath.Length <= settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength);
                    valid &= topicQueueNameRegex.IsMatch(entityPath);
                    break;
                case EntityType.Subscription:
                    valid &= entityPath.Length <= settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength);
                    valid &= subscriptionNameRegex.IsMatch(entityPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }
            
            return valid;
        }
    }
}