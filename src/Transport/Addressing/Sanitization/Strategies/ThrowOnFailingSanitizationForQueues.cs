namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;
    using Settings;

    public class ThrowOnFailingSanitizationForQueues : SanitizationStrategy
    {
        ReadOnlySettings settings;
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        Regex queuePathRegex = new Regex(@"^[^\/][0-9A-Za-z_\.\-\/]+[^\/]$");
        
        public ThrowOnFailingSanitizationForQueues(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public override EntityType CanSanitize { get; } = EntityType.Queue;

        public override string Sanitize(string entityPath)
        {
            var maxLength = settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
            if (entityPath.Length > maxLength)
            {
                throw new Exception($"Invalid queue path `{entityPath}` that cannot be used with Azure Service Bus. Path exceeds maximum length of {maxLength} characters."
                     + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            if (!queuePathRegex.IsMatch(entityPath))
            {
                {
                    throw new Exception($"Invalid queue path `{entityPath}` that cannot be used with Azure Service Bus. Path contains illegal characters. Legal characters should match the following regex: `{queuePathRegex}`."
                         + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
                }
            }

            return entityPath;
        }
    }
}