namespace NServiceBus.AzureServiceBus.Addressing
{
    using Topology.MetaModel;

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
            var address = new EntityAddress(entityname);
            entityname = address.Name;

            var path = composition.GetEntityPath(entityname, entityType);
            var validationResult = validationStrategy.IsValid(path, entityType);
            if (!validationResult.IsValid)
            {
                path = sanitizationStrategy.Sanitize(path, entityType);
            }
            return path;
        }
    }
}
