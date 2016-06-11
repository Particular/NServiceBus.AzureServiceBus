namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailingSanitizationForTopics : SanitizationStrategy
    {
        ReadOnlySettings settings;
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex topicPathRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");

        public ThrowOnFailingSanitizationForTopics(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public override EntityType CanSanitize { get; } = EntityType.Topic;

        public override string Sanitize(string entityPath)
        {
            var maxLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathMaximumLength);
            if (entityPath.Length > maxLength)
            {
                throw new Exception($"Invalid topic path `{entityPath}` that cannot be used with Azure Service Bus. Path exceeds maximum length of {maxLength} characters."
                     + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            if (!topicPathRegex.IsMatch(entityPath))
            {
                {
                    throw new Exception($"Invalid topic path `{entityPath}` that cannot be used with Azure Service Bus. Path contains illegal characters. Legal characters should match the following regex: `{topicPathRegex}`."
                         + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
                }
            }

            return entityPath;
        }
    }
}