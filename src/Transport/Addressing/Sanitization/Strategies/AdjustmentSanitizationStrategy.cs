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

        public string Sanitize(string endpointname, EntityType type)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._\/]");
            endpointname = rgx.Replace(endpointname, "");

            if (!_validationStrategy.IsValid(endpointname, type))
            {
                // turn long name into a guid
                endpointname = new DeterministicGuidBuilder().Build(endpointname).ToString();
            }

            return endpointname;
        }
    }
}