namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;

    public class AdjustmentSanitizationV6 : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public AdjustmentSanitizationV6(IValidationStrategy validationStrategy)
        {
            this.validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPath, EntityType entityType)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
            entityPath = rgx.Replace(entityPath, "");

            if (!validationStrategy.IsValid(entityPath, entityType))
            {
                // turn long name into a guid
                entityPath = MD5DeterministicNameBuilder.Build(entityPath);
            }

            return entityPath;
        }
    }

}