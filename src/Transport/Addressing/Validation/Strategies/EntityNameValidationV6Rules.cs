namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class EntityNameValidationV6Rules : IValidationStrategy
    {
        ReadOnlySettings settings;

        public EntityNameValidationV6Rules(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public bool IsValid(string entityPath, EntityType entityType)
        {
            // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
            // Except for subscriptions, these cannot contain slashes (/)
            var v6PathRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

            var isValid = false;
            switch (entityType)
            {
                case EntityType.Queue:
                    isValid = entityPath.Length <= settings.Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.QueuePathMaximumLength);
                    isValid &= v6PathRegex.IsMatch(entityPath);
                    break;
                case EntityType.Topic:
                    isValid = entityPath.Length <= settings.Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.TopicPathMaximumLength);
                    isValid &= v6PathRegex.IsMatch(entityPath);
                    break;
                case EntityType.Subscription:
                    isValid = entityPath.Length <= settings.Get<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.SubscriptionPathMaximumLength);
                    isValid &= v6PathRegex.IsMatch(entityPath);
                    break;
                case EntityType.Rule:
                    isValid &= entityPath.Length <= settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Validation.RuleNameMaximumLength);
                    isValid &= v6PathRegex.IsMatch(entityPath);
                    break;
                case EntityType.EventHub:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            return isValid;
        }
    }
}