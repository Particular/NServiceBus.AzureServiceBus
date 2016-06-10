namespace NServiceBus.AzureServiceBus.Addressing
{
    using Topology.MetaModel;

    class AddressingLogic
    {
        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;

        public AddressingLogic(ISanitizationStrategy sanitizationStrategy, ICompositionStrategy composition)
        {
            this.sanitizationStrategy = sanitizationStrategy;
            this.composition = composition;
        }

        public string Apply(string entityname, EntityType entityType)
        {
            var address = new EntityAddress(entityname);
            entityname = address.Name;

            var path = composition.GetEntityPath(entityname, entityType);
            path = sanitizationStrategy.Sanitize(path, entityType);
            return path;
        }
    }
}
