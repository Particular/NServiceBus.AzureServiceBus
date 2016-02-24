namespace NServiceBus.AzureServiceBus
{
    using System;
    public class NamespaceDefinition : IEquatable<NamespaceDefinition>
    {
        public string Name { get; }
        public string ConnectionString { get; }

        public NamespaceDefinition(string name, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Namespace name can't be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Namespace connection string can't be null or empty", nameof(connectionString));

            Name = name;
            ConnectionString = connectionString;
        }

        public bool Equals(NamespaceDefinition other)
        {
            return other != null
                   && Name.Equals(other.Name)
                   && ConnectionString.Equals(other.ConnectionString);
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceDefinition;
            return this.Equals(target);
        }

        public override int GetHashCode()
        {
            return String.Concat(Name, "-", ConnectionString).GetHashCode();
        }
    }
}