namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class RuntimeNamespaceInfo : IEquatable<RuntimeNamespaceInfo>
    {
        readonly NamespaceInfo info;

        public RuntimeNamespaceInfo(string alias, string connectionString, NamespacePurpose purpose = NamespacePurpose.Partitioning, NamespaceMode mode = NamespaceMode.Active)
        {
            info = new NamespaceInfo(alias, connectionString, purpose);
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