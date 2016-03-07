namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Logging;

    public class NamespaceConfigurations : IEnumerable<NamespaceInfo>
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(NamespaceConfigurations));

        internal static readonly string DefaultName = "default";

        private readonly List<NamespaceInfo> _inner;

        public NamespaceConfigurations()
        {
            _inner = new List<NamespaceInfo>();
        }

        public int Count => _inner.Count;

        public void Add(string name, string connectionString)
        {
            var definition = new NamespaceInfo(name, connectionString);

            var namespaceInfo = _inner.SingleOrDefault(x => x.ConnectionString == definition.ConnectionString);
            if (namespaceInfo != null)
            {
                Log.Info($"Duplicated connection string for `{namespaceInfo.Name}` and `{name}` namespaces." + Environment.NewLine +
                         $"`{name}` namespace not registered");
                return;
            }

            if (_inner.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Info($"Duplicated namespace name `{name}` configuration detected. Registered only once");
                return;
            }

            _inner.Add(definition);
        }

        public void AddDefault(string connectionString)
        {
            Add(DefaultName, connectionString);
        }

        public string GetConnectionString(string name)
        {
            try
            {
                var selected = _inner.Single(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return selected.ConnectionString;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Namespace with name `{name}` hasn't been registered", ex);
            }
        }

        public IEnumerator<NamespaceInfo> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}