namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Settings;

    class DefaultNamespaceNameToConnectionStringMapper : ICanMapNamespaceNameToConnectionString
    {
        private readonly ReadOnlySettings _settings;

        public DefaultNamespaceNameToConnectionStringMapper(ReadOnlySettings settings)
        {
            this._settings = settings;
        }

        public EntityAddress Map(EntityAddress value)
        {
            if (!value.HasSuffix) return value;

            var namespaces = _settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);

            var namespaceInfo = namespaces.SingleOrDefault(x => x.Name == value.Suffix);
            if (namespaceInfo != null)
                return new EntityAddress($"{value.Name}@{namespaceInfo.ConnectionString}");

            throw new KeyNotFoundException();
        }
    }
}