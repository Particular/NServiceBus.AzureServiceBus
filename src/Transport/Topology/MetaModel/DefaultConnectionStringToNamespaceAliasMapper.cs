namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Linq;
    using Logging;
    using Settings;

    class DefaultConnectionStringToNamespaceAliasMapper
    {
        public DefaultConnectionStringToNamespaceAliasMapper(ReadOnlySettings settings)
        {
            namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
        }

        public virtual EntityAddress Map(EntityAddress value)
        {
            if (!value.HasConnectionString)
            {
                return value;
            }

            var namespaceInfo = namespaces.SingleOrDefault(x => x.Connection == value.Suffix);
            if (namespaceInfo != null)
            {
                return new EntityAddress($"{value.Name}@{namespaceInfo.Alias}");
            }

            var namespaceName = new ConnectionStringInternal(value.Suffix).NamespaceName;
            logger.Warn($"Connection string for for namespace name '{namespaceName}' hasn't been configured. {Environment.NewLine}, replying may not work properly" +
                        "Use `AddNamespace` configuration API to map connection string to namespace alias.");
            return value;
        }

        static ILog logger = LogManager.GetLogger<DefaultConnectionStringToNamespaceAliasMapper>();
        NamespaceConfigurations namespaces;
    }
}