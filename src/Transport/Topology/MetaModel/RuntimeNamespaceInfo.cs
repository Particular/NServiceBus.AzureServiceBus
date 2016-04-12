namespace NServiceBus.AzureServiceBus
{
    using System;

    public class RuntimeNamespaceInfo : IEquatable<RuntimeNamespaceInfo>
    {
        NamespaceInfo info;

        public RuntimeNamespaceInfo(string name, string connectionString, NamespaceMode mode = NamespaceMode.Active)
        {
            info = new NamespaceInfo(name, connectionString);
            Mode = mode;
        }

        public string Name => info.Name;
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