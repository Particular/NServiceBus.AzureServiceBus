namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    public class NamespaceConfigurations : IEnumerable<NamespaceInfo>
    {
        static ILog Log = LogManager.GetLogger(typeof(NamespaceConfigurations));

     //   internal static Func<string> DefaultName = () => "default";

        List<NamespaceInfo> inner;

        public NamespaceConfigurations()
        {
            inner = new List<NamespaceInfo>();
        }

        public int Count => inner.Count;

        public void Add(string name, string connectionString, NamespacePurpose purpose = NamespacePurpose.Partitioning)
        {
            var definition = new NamespaceInfo(name, connectionString, purpose);

            var namespaceInfo = inner.SingleOrDefault(x => x.ConnectionString == definition.ConnectionString);
            if (namespaceInfo != null)
            {
                Log.Info($"Duplicated connection string for `{namespaceInfo.Name}` and `{name}` namespaces." + Environment.NewLine +
                         $"`{name}` namespace not registered");
                return;
            }

            if (inner.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Info($"Duplicated namespace name `{name}` configuration detected. Registered only once");
                return;
            }

            inner.Add(definition);
        }

        public string GetConnectionString(string name)
        {
            try
            {
                var selected = inner.Single(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return selected.ConnectionString;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Namespace with name `{name}` hasn't been registered", ex);
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