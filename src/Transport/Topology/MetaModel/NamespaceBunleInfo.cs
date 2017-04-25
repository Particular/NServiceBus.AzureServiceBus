namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;

    class NamespaceBunleInfo : IEquatable<NamespaceBunleInfo>
    {
        public string Alias { get; }
        public int NumberOfTopicsInBundle { get; }

        public NamespaceBunleInfo(string alias, int numberOfTopicsInBundle)
        {
            Alias = alias;
            NumberOfTopicsInBundle = numberOfTopicsInBundle;
        }

        public bool Equals(NamespaceBunleInfo other)
        {
            return other != null
                   && Alias.Equals(other.Alias, StringComparison.OrdinalIgnoreCase)
                   && NumberOfTopicsInBundle == other.NumberOfTopicsInBundle;
        }

        public override bool Equals(object obj)
        {
            var target = obj as NamespaceBunleInfo;
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