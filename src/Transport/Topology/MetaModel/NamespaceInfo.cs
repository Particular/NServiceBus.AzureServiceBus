namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class NamespaceInfo : IEquatable<NamespaceInfo>
    {
        public string Alias { get; }
        public ConnectionString ConnectionString { get; }
        public NamespacePurpose Purpose { get; }

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
            ConnectionString = new ConnectionString(connectionString);
            Purpose = purpose;
        }

        public bool Equals(NamespaceInfo other)
        {
            return other != null
                   && Alias.Equals(other.Alias, StringComparison.OrdinalIgnoreCase)
                   && ConnectionString.Equals(other.ConnectionString);
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            var name = Alias.ToLower();
            return string.Concat(name, "#", ConnectionString).GetHashCode();
        }
    }
}