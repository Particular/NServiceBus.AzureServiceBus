namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Linq;

    class EntityAddress
    {
        string value;

        public string Name => value.Split('@').First();
        public string Suffix => value.Contains('@') ? value.Split('@').Last() : string.Empty;
        public bool HasConnectionString
        {
            get
            {
                ConnectionString connectionString;
                return ConnectionString.TryParse(Suffix, out connectionString);
            }
        }
        public bool HasSuffix => !string.IsNullOrWhiteSpace(Suffix);

        public EntityAddress(string name, string suffix) : this(name + "@" + suffix) { }

        public EntityAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Entity address value can't be empty", nameof(value));

            this.value = value;
        }

        public static implicit operator EntityAddress(string value)
        {
            return new EntityAddress(value);
        }

        public static implicit operator string(EntityAddress address)
        {
            return address.value;
        }
    }
}
