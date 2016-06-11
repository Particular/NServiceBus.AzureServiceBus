namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailingSanitizationForSubscription : SanitizationStrategy
    {
        ReadOnlySettings settings;
        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex subscriptionNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public ThrowOnFailingSanitizationForSubscription(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public override EntityType CanSanitize { get; } = EntityType.Subscription;

        public override string Sanitize(string entityName)
        {
            var maxLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionPathMaximumLength);
            if (entityName.Length > maxLength)
            {
                throw new Exception($"Invalid subscription name `{entityName}` that cannot be used with Azure Service Bus. Name exceeds maximum length of {maxLength} characters."
                     + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            if (!subscriptionNameRegex.IsMatch(entityName))
            {
                {
                    throw new Exception($"Invalid subscription name `{entityName}` that cannot be used with Azure Service Bus. Name contains illegal characters. Legal characters should match the following regex: `{subscriptionNameRegex}`."
                         + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
                }
            }

            return entityName;
        }
    }
}