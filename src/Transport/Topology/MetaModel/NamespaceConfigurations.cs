namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class NamespaceConfigurations : IEnumerable<NamespaceInfo>
    {
        static ILog Log = LogManager.GetLogger(typeof(NamespaceConfigurations));

        List<NamespaceInfo> inner;

        public NamespaceConfigurations()
        {
            inner = new List<NamespaceInfo>();
        }

        internal NamespaceConfigurations(List<NamespaceInfo> configurations)
        {
            inner = configurations;
        }

        public int Count => inner.Count;

        public void Add(string alias, string connectionString, NamespacePurpose purpose)
        {
            var definition = new NamespaceInfo(alias, connectionString, purpose);

            var namespaceInfo = inner.SingleOrDefault(x => x.ConnectionString == definition.ConnectionString);
            if (namespaceInfo != null)
            {
                Log.Info($"Duplicated connection string for namespace `{namespaceInfo.Alias}` and alias `{alias}.`  + {Environment.NewLine} + `{alias}` namespace alias was not registered.");
                return;
            }

            if (inner.Any(x => string.Equals(x.Alias, alias, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Info($"Duplicated namespace alias `{alias}` configuration detected. Registered only once");
                return;
            }

            inner.Add(definition);
        }

        public string GetConnectionString(string name)
        {
            try
            {
                var selected = inner.Single(x => x.Alias.Equals(name, StringComparison.OrdinalIgnoreCase));
                return selected.ConnectionString;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Namespace with alias `{name}` hasn't been registered", ex);
            }
        }

        public IEnumerator<NamespaceInfo> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}