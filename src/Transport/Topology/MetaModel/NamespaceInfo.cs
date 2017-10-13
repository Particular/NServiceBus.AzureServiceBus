namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    public partial class NamespaceInfo : IEquatable<NamespaceInfo>
    {
        /// <summary></summary>
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

        /// <summary></summary>
        public string Alias { get; }

        /// <summary></summary>
        public NamespacePurpose Purpose { get; }

        /// <summary></summary>
        public string Connection => connectionString;

        /// <summary></summary>
        public HashSet<string> RegisteredEndpoints { get; }

        /// <summary></summary>
        public bool Equals(NamespaceInfo other)
        {
            return other != null
                   && Alias.Equals(other.Alias, StringComparison.OrdinalIgnoreCase)
                   && connectionString.Equals(other.connectionString);
        }

        /// <summary></summary>
        public override bool Equals(object obj)
        {
            var target = obj as NamespaceInfo;
            return Equals(target);
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            var name = Alias.ToLower();
            return string.Concat(name, "#", connectionString).GetHashCode();
        }

        ConnectionStringInternal connectionString;
    }
}