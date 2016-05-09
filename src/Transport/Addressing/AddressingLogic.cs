namespace NServiceBus.AzureServiceBus.Addressing
{
    class AddressingLogic
    {
        readonly IValidationStrategy validationStrategy;
        readonly ISanitizationStrategy sanitizationStrategy;
        readonly ICompositionStrategy composition;

        public AddressingLogic(IValidationStrategy validationStrategy, ISanitizationStrategy sanitizationStrategy, ICompositionStrategy composition)
        {
            this.validationStrategy = validationStrategy;
            this.sanitizationStrategy = sanitizationStrategy;
            this.composition = composition;
        }

        public string Apply(string entityname, EntityType entityType)
        {
            var path = composition.GetEntityPath(entityname, entityType);
            if (!validationStrategy.IsValid(path, entityType))
            {
                path = sanitizationStrategy.Sanitize(path, entityType);
            }
            return path;
        }
    }
}
