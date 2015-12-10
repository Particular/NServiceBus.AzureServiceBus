namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;

    public class EntityInfo
    {
        public string Path { get; set; }

        public EntityType Type { get; set; }

        public NamespaceInfo Namespace { get; set; }

        public IList<EntityRelationShipInfo> RelationShips { get; private set; }
        public string Metadata { get; set; }

        public EntityInfo()
        {
            RelationShips = new List<EntityRelationShipInfo>();
        }

        protected bool Equals(EntityInfo other)
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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((EntityInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Type;
                hashCode = (hashCode*397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EntityInfo left, EntityInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityInfo left, EntityInfo right)
        {
            return !Equals(left, right);
        }
    }
}