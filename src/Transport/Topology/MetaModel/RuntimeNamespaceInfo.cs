namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    /// <summary></summary>
    public class RuntimeNamespaceInfo : IEquatable<RuntimeNamespaceInfo>
    {
        /// <summary></summary>
        public RuntimeNamespaceInfo(string alias, string connectionString, NamespacePurpose purpose = NamespacePurpose.Partitioning, NamespaceMode mode = NamespaceMode.Active)
        {
            info = new NamespaceInfo(alias, connectionString, purpose);
            Mode = mode;
        }

        /// <summary></summary>
        public string Alias => info.Alias;

        /// <summary></summary>
        public string ConnectionString => info.Connection;

        /// <summary></summary>
        public NamespaceMode Mode { get; }

        /// <summary></summary>
        public bool Equals(RuntimeNamespaceInfo other) => other != null && info.Equals(other.info);

        /// <summary></summary>
        public override bool Equals(object obj)
        {
            var target = obj as RuntimeNamespaceInfo;
            return Equals(target);
        }

        /// <summary></summary>
        public override int GetHashCode() => info.GetHashCode();

        /// <summary></summary>
        public static bool operator ==(RuntimeNamespaceInfo left, RuntimeNamespaceInfo right)
        {
            return Equals(left, right);
        }

        /// <summary></summary>
        public static bool operator !=(RuntimeNamespaceInfo left, RuntimeNamespaceInfo right)
        {
            return !(left == right);
        }

        readonly NamespaceInfo info;
    }
}