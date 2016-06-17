namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using Settings;

    public class ThrowOnFailedValidation : ISanitizationStrategy
    {
        ReadOnlySettings settings;
   
        public ThrowOnFailedValidation(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            Func<string, ValidationResult> validator;

            switch (entityType)
            {
                case EntityType.Queue:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.QueuePathValidator);
                    break;

                case EntityType.Topic:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.TopicPathValidator);
                    break;

                case EntityType.Subscription:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.SubscriptionNameValidator);
                    break;

                case EntityType.Rule:
                    validator = settings.GetOrDefault<Func<string, ValidationResult>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.RuleNameValidator);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }

            var validationResult = validator(entityPathOrName);
            if (!validationResult.CharactersAreValid)
                ThrowException(entityPathOrName, entityType, validationResult.CharactersError);

            if (!validationResult.LengthIsValid)
                ThrowException(entityPathOrName, entityType, validationResult.LengthError);

            return entityPathOrName;
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