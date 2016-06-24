namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
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
            if (!value.HasSuffix || value.HasConnectionString) return value;

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);

            var namespaceInfo = namespaces.SingleOrDefault(x => x.Name == value.Suffix);
            if (namespaceInfo != null)
            {
                return new EntityAddress($"{value.Name}@{namespaceInfo.ConnectionString}");
            }

            throw new InvalidOperationException($"Provided namespace name ({value.Suffix}) hasn't been configured. {Environment.NewLine}" +
                                                "Use `AddNamespace` configuration API to map namespace name to connection string.");
        }
    }
}