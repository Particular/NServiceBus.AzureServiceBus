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
            return new EntityAddress(value);
        }
    }
}