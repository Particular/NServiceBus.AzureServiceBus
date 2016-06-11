namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailingSanitizationForRules : SanitizationStrategy
    {
        ReadOnlySettings settings;
        // Except for subscriptions and rules, these cannot contain slashes (/)
        Regex ruleNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");

        public ThrowOnFailingSanitizationForRules(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public override EntityType CanSanitize { get; } = EntityType.Rule;

        public override string Sanitize(string entityName)
        {
            var maxLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameMaximumLength);
            if (entityName.Length > maxLength)
            {
                throw new Exception($"Invalid rule name `{entityName}` that cannot be used with Azure Service Bus. Name exceeds maximum length of {maxLength} characters."
                     + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            if (!ruleNameRegex.IsMatch(entityName))
            {
                {
                    throw new Exception($"Invalid rule name `{entityName}` that cannot be used with Azure Service Bus. Name contains illegal characters. Legal characters should match the following regex: `{ruleNameRegex}`."
                         + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
                }
            }

            return entityName;
        }
    }
}