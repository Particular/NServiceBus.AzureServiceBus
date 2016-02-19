namespace NServiceBus.AzureServiceBus
{
    using System;

    // NOTE: Can namespaces switch mode?
    public class NamespaceInfo : IEquatable<NamespaceInfo>
    {
        private readonly NamespaceDefinition _definition;

        public NamespaceInfo(string name, string connectionString, NamespaceMode mode = NamespaceMode.Active)
        {
            _definition = new NamespaceDefinition(name, connectionString);
            Mode = mode;
        }

        public string Name => _definition.Name;
        public string ConnectionString => _definition.ConnectionString;
        public NamespaceMode Mode { get; }

        public bool Equals(NamespaceInfo other)
        {
            return other != null && _definition.Equals(other._definition);
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _definition.GetHashCode(); 
        }

        public static bool operator ==(NamespaceInfo left, NamespaceInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamespaceInfo left, NamespaceInfo right)
        {
            return !(left == right);
        }
    }
}