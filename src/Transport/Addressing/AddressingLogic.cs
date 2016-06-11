namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Linq;
    using Settings;
    using Topology.MetaModel;

    class AddressingLogic
    {
        readonly ICompositionStrategy composition;
        readonly SettingsHolder settings;

        public AddressingLogic(SettingsHolder settings, ICompositionStrategy composition)
        {
            this.settings = settings;
            this.composition = composition;
        }

        public string Apply(string entityPathOrName, EntityType entityType)
        {
            var address = new EntityAddress(entityPathOrName);
            entityPathOrName = address.Name;

            var pathOrName = composition.GetEntityPath(entityPathOrName, entityType);
            var allStrategies = settings.Get<HashSet<SanitizationStrategy>>(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            var strategiesForEntityType = allStrategies.Where(x => x.CanSanitize == entityType);
            foreach (var strategy in strategiesForEntityType)
            {
                pathOrName = strategy.Sanitize(pathOrName);
            }

            return pathOrName;
        }
    }
}
