namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    struct EntityAddress : IEquatable<EntityAddress>
    {
        public bool Equals(EntityAddress other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Suffix, other.Suffix);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is EntityAddress && Equals((EntityAddress)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ (Suffix?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(EntityAddress left, EntityAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityAddress left, EntityAddress right)
        {
            return !left.Equals(right);
        }

        public EntityAddress(string name, string suffix)
        {
            Name = name;
            Suffix = suffix;

            HasSuffix = !string.IsNullOrWhiteSpace(Suffix);
            HasConnectionString = ConnectionStringInternal.TryParse(Suffix, out var _);
        }

        public EntityAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Entity address value can't be empty", nameof(value));
            }

            var splitByAt = value.Split(seperators);

            if (splitByAt.Length == 1)
            {
                Name = splitByAt[0];
                Suffix = string.Empty;
            }
            else
            {
                Name = splitByAt[0];
                Suffix = splitByAt[splitByAt.Length - 1];
            }

            HasSuffix = !string.IsNullOrWhiteSpace(Suffix);
            HasConnectionString = ConnectionStringInternal.TryParse(Suffix, out var _);
        }

        public string Name { get; }
        public string Suffix { get; }
        public bool HasConnectionString { get; }
        public bool HasSuffix { get; }

        public override string ToString()
        {
            return HasSuffix ? $"{Name}@{Suffix}" : Name;
        }

        static char[] seperators =
        {
            '@'
        };
    }
}