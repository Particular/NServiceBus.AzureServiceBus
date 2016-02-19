namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class NamespacesDefinition : IEnumerable<NamespaceDefinition>
    {
        private static readonly string DefaultName = "default";
        private static readonly string Prefix = "namespace-";

        private readonly List<NamespaceDefinition> _inner;

        public NamespacesDefinition()
        {
            _inner = new List<NamespaceDefinition>();
        }

        public int Count => _inner.Count;

        public void Add(string name, string connectionString)
        {
            var definition = new NamespaceDefinition(name, connectionString);
            if (_inner.Contains(definition)) return;

            var defaultDefinition = _inner.SingleOrDefault(x => x.Name == DefaultName && x.ConnectionString == connectionString);
            _inner.Remove(defaultDefinition);
            _inner.Add(definition);
        }

        public void Add(string connectionString)
        {
            var name = $"{Prefix}{_inner.Count + 1}";
            Add(name, connectionString);
        }

        public void AddDefault(string connectionString)
        {
            var definition = new NamespaceDefinition(DefaultName, connectionString);
            if (_inner.Any(x => x.ConnectionString == connectionString)) return;

            _inner.Add(definition);
        }

        public string GetConnectionString(string name)
        {
            try
            {
                var selected = _inner.Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                return selected.ConnectionString;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Namespace with name {name} hasn't been registered", ex);
            }
        }

        public IEnumerator<NamespaceDefinition> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}