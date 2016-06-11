﻿namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;
    using Settings;

    public class EndpointOrientedTopologySanitizationForQueues : SanitizationStrategy
    {
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/)
        // Except for subscriptions, these cannot contain slashes (/)
        Regex v6PathRegex = new Regex(@"[^a-zA-Z0-9\-\._]");
        readonly ReadOnlySettings settings;

        public EndpointOrientedTopologySanitizationForQueues(ReadOnlySettings settings)
        {
            this.settings = settings;
        }
        public override EntityType CanSanitize { get; } = EntityType.Queue;

        public override string Sanitize(string entityPath)
        {
            // remove characters invalid in v6
            entityPath = v6PathRegex.Replace(entityPath, "");

            var shouldHash = entityPath.Length > settings.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathMaximumLength);
            if (shouldHash)
            {
                return MD5DeterministicNameBuilder.Build(entityPath);
            }
            return entityPath;
        }
    }
}