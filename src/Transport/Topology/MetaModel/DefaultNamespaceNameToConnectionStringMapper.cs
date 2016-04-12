namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System.Collections.Generic;
    using System.Linq;
    using Settings;

    class DefaultNamespaceNameToConnectionStringMapper : ICanMapNamespaceNameToConnectionString
    {
        ReadOnlySettings settings;

        public DefaultNamespaceNameToConnectionStringMapper(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public EntityAddress Map(EntityAddress value)
        {
            if (!value.HasSuffix) return value;

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);

            var namespaceInfo = namespaces.SingleOrDefault(x => x.Name == value.Suffix);
            if (namespaceInfo != null)
            {
                return new EntityAddress($"{value.Name}@{namespaceInfo.ConnectionString}");
            }

            throw new KeyNotFoundException();
        }
    }
}