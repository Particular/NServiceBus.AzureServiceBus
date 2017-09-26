namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    public partial class NamespaceInfo : IEquatable<NamespaceInfo>
    {
        public NamespaceInfo(string alias, string connectionString, NamespacePurpose purpose = NamespacePurpose.Partitioning)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Namespace alias can't be null or empty", nameof(alias));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Namespace connection string can't be null or empty", nameof(connectionString));
            }

            Alias = alias;
            this.connectionString = new ConnectionStringInternal(connectionString);
            Purpose = purpose;
            RegisteredEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Alias { get; }

        public NamespacePurpose Purpose { get; }

        public string Connection => connectionString;

        public HashSet<string> RegisteredEndpoints { get; }

        public bool Equals(NamespaceInfo other)
        {
            return other != null
                   && Alias.Equals(other.Alias, StringComparison.OrdinalIgnoreCase)
                   && connectionString.Equals(other.connectionString);
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            var name = Alias.ToLower();
            return string.Concat(name, "#", connectionString).GetHashCode();
        }

        ConnectionStringInternal connectionString;
    }
}