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

        public string Sanitize(string endpointName, EntityType entityType)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._\/]");
            endpointName = rgx.Replace(endpointName, "");

            if (!_validationStrategy.IsValid(endpointName, entityType))
            {
                // turn long name into a guid
                endpointName = new DeterministicGuidBuilder().Build(endpointName).ToString();
            }

            return endpointName;
        }
    }
}