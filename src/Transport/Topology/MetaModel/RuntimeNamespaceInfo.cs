namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    public class RuntimeNamespaceInfo : IEquatable<RuntimeNamespaceInfo>
    {
        readonly NamespaceInfo info;

        public RuntimeNamespaceInfo(string alias, string connectionString, NamespacePurpose purpose = NamespacePurpose.Partitioning, NamespaceMode mode = NamespaceMode.Active) : this(new NamespaceInfo(alias, connectionString, purpose), mode)
        {
        }

        internal RuntimeNamespaceInfo(NamespaceInfo info, NamespaceMode mode = NamespaceMode.Active)
        {
            this.info = info;
            Mode = mode;
        }

        public string Alias => info.Alias;
        public string ConnectionString => info.ConnectionString;
        public NamespaceMode Mode { get; }

        public bool Equals(RuntimeNamespaceInfo other)
        {
            return other != null && info.Equals(other.info);
        }

        public override bool Equals(object obj)
        {
            var target = obj as RuntimeNamespaceInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return info.GetHashCode();
        }

        public static bool operator ==(RuntimeNamespaceInfo left, RuntimeNamespaceInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RuntimeNamespaceInfo left, RuntimeNamespaceInfo right)
        {
            return !(left == right);
        }
    }
}