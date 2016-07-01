namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System.Linq;
    using Settings;

    class PassThroughNamespaceNameToConnectionStringMapper : ICanMapNamespaceNameToConnectionString
    {
        ReadOnlySettings settings;

        public PassThroughNamespaceNameToConnectionStringMapper(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public EntityAddress Map(EntityAddress value)
        {
            if (!value.HasSuffix)
            {
                var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
                var defaultName = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceName);
                var selected = namespaces.FirstOrDefault(ns => ns.Name == defaultName);
                if (selected == null)
                {
                    selected = namespaces.FirstOrDefault(ns => ns.Purpose == NamespacePurpose.Partitioning);
                }

                if (selected != null)
                {
                    return new EntityAddress(value.Name, selected.Name);
                }

            }

            return new EntityAddress(value);
        }
    }
}