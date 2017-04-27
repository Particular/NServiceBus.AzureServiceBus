namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;

    class NamespaceBundleInfo : IEquatable<NamespaceBundleInfo>
    {
        public string Alias { get; }
        public int NumberOfTopicsInBundle { get; }

        public NamespaceBundleInfo(string alias, int numberOfTopicsInBundle)
        {
            Alias = alias;
            NumberOfTopicsInBundle = numberOfTopicsInBundle;
        }

        public bool Equals(NamespaceBundleInfo other)
        {
            return other != null
                   && Alias.Equals(other.Alias, StringComparison.OrdinalIgnoreCase)
                   && NumberOfTopicsInBundle == other.NumberOfTopicsInBundle;
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceBundleInfo;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Alias != null ? Alias.GetHashCode() : 0) * 397) ^ NumberOfTopicsInBundle;
            }
        }
    }
}