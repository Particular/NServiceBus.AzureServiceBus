namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Linq;
    using Logging;
    using Settings;

    class DefaultConnectionStringToNamespaceAliasMapper
    {
        ReadOnlySettings settings;
        static ILog Logger = LogManager.GetLogger<DefaultConnectionStringToNamespaceAliasMapper>();

        public DefaultConnectionStringToNamespaceAliasMapper(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public virtual EntityAddress Map(EntityAddress value)
        {
            if (!value.HasConnectionString)
            {
                return value;
            }

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);

            var namespaceInfo = namespaces.SingleOrDefault(x => x.Connection == value.Suffix);
            if (namespaceInfo != null)
            {
                return new EntityAddress($"{value.Name}@{namespaceInfo.Alias}");
            }

            var namespaceName = new ConnectionStringInternal(value.Suffix).NamespaceName;
            Logger.Warn($"Connection string for for namespace name '{namespaceName}' hasn't been configured. {Environment.NewLine}, replying may not work properly" +
                                                "Use `AddNamespace` configuration API to map connection string to namespace alias.");
            return value;
        }
    }
}