namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;

    public class AdjustmentSanitizationStrategy : ISanitizationStrategy
    {
        IValidationStrategy _validationStrategy;

        public AdjustmentSanitizationStrategy(IValidationStrategy validationStrategy)
        {
            _validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPath, EntityType entityType)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._\/]");
            entityPath = rgx.Replace(entityPath, "");

            if (!_validationStrategy.IsValid(entityPath, entityType))
            {
                // turn long name into a guid
                entityPath = new DeterministicGuidBuilder().Build(entityPath).ToString();
            }

            return entityPath;
        }
    }
}