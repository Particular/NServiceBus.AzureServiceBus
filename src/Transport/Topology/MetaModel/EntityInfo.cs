namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    class EntityInfoInternal
    {
        public string Path { get; set; }

        public EntityType Type { get; set; }

        public RuntimeNamespaceInfo Namespace { get; set; }

        public IList<EntityRelationShipInfoInternal> RelationShips { get; private set; }

        public bool ShouldBeListenedTo { get; set; } = true;

        public EntityInfoInternal()
        {
            RelationShips = new List<EntityRelationShipInfoInternal>();
        }

        protected bool Equals(EntityInfoInternal other)
        {
            return string.Equals(Path, other.Path) && Type == other.Type && Equals(Namespace, other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((EntityInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Path?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (int) Type;
                hashCode = (hashCode*397) ^ (Namespace?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(EntityInfoInternal left, EntityInfoInternal right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityInfoInternal left, EntityInfoInternal right)
        {
            return !Equals(left, right);
        }
    }
}