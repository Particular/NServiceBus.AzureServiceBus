namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    /// <summary> </summary>
    public class NamespaceConfigurations : IEnumerable<NamespaceInfo>
    {
        /// <summary> </summary>
        public NamespaceConfigurations()
        {
            inner = new List<NamespaceInfo>();
        }

        internal NamespaceConfigurations(List<NamespaceInfo> configurations)
        {
            inner = configurations;
        }

        /// <summary> </summary>
        public int Count => inner.Count;

        /// <summary> </summary>
        public IEnumerator<NamespaceInfo> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary> </summary>
        public NamespaceInfo Add(string alias, string connectionString, NamespacePurpose purpose)
        {
            var definition = new NamespaceInfo(alias, connectionString, purpose);
            var namespaceInfo = inner.SingleOrDefault(x => x.Connection == definition.Connection);
            if (namespaceInfo != null)
            {
                Log.Info($"Duplicated connection string for namespace `{namespaceInfo.Alias}` and alias `{alias}.`  + {Environment.NewLine} + `{alias}` namespace alias was not registered.");
                return namespaceInfo;
            }

            namespaceInfo = inner.SingleOrDefault(x => string.Equals(x.Alias, alias, StringComparison.OrdinalIgnoreCase));
            if (namespaceInfo != null)
            {
                Log.Info($"Duplicated namespace alias `{alias}` configuration detected. Registered only once");
                return namespaceInfo;
            }

            inner.Add(definition);
            return definition;
        }

        /// <summary> </summary>
        public string GetConnectionString(string name)
        {
            try
            {
                var selected = inner.Single(x => x.Alias.Equals(name, StringComparison.OrdinalIgnoreCase));
                return selected.Connection;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Namespace with alias `{name}` hasn't been registered", ex);
            }
        }

        List<NamespaceInfo> inner;
        static ILog Log = LogManager.GetLogger(typeof(NamespaceConfigurations));
    }
}