namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;

    class SubscriptionInfoInternal : EntityInfoInternal
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        public IClientSideSubscriptionFilterInternal ClientSideFilter { get; set; }

        public SubscriptionMetadataInternal Metadata { get; set; }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }

            var other = obj as SubscriptionInfoInternal;
            if (other == null)
            {
                return false;
            }

            var entity = RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
            var otherEntity = other.RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
            var targetPathEquals = string.Equals(entity.Target.Path, otherEntity.Target.Path);

            // both target the same topic
            return targetPathEquals;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                var entity = RelationShips.First(r => r.Type == EntityRelationShipTypeInternal.Subscription);
                hashCode = (hashCode * 397) ^ entity.Target.Path.GetHashCode();
                return hashCode;
            }
        }
    }
}