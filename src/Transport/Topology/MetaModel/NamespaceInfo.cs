namespace NServiceBus.AzureServiceBus
{
    using System;
    using Topology.MetaModel;

    public class NamespaceInfo : IEquatable<NamespaceInfo>
    {
        public string Name { get; }
        public ConnectionString ConnectionString { get; }

        public NamespaceInfo(string name, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Namespace name can't be null or empty", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Namespace connection string can't be null or empty", nameof(connectionString));
            }

            Name = name;
            ConnectionString = new ConnectionString(connectionString);
        }

        public bool Equals(NamespaceInfo other)
        {
            return other != null
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && ConnectionString.Equals(other.ConnectionString);
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            var name = Name.ToLower();
            return string.Concat(name, "#", ConnectionString).GetHashCode();
        }
    }
}