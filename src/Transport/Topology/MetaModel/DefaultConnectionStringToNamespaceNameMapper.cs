namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Settings;

    class DefaultConnectionStringToNamespaceNameMapper : ICanMapConnectionStringToNamespaceName
    {
        ReadOnlySettings settings;

        public DefaultConnectionStringToNamespaceNameMapper(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public EntityAddress Map(EntityAddress value)
        {
            if (!value.HasConnectionString)
            {
                return value;
            }

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);

            var namespaceInfo = namespaces.SingleOrDefault(x => x.ConnectionString == value.Suffix);
            if (namespaceInfo != null)
            {
                return new EntityAddress($"{value.Name}@{namespaceInfo.Name}");
            }

            throw new KeyNotFoundException();
        }
    }
}