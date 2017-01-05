namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class SubscriptionInfo : EntityInfo
    {
        public IBrokerSideSubscriptionFilter BrokerSideFilter { get; set; }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public IClientSideSubscriptionFilter ClientSideFilter { get; set; }

        public SubscriptionMetadata Metadata { get; set; }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;

            var other = obj as SubscriptionInfo;
            if (other == null)
            {
                return false;
            }

            var entity = RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
            var otherEntity = other.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
            var targetPathEquals = string.Equals(entity.Target.Path, otherEntity.Target.Path);

            // both target the same topic
            return targetPathEquals;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                var entity = RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                hashCode = (hashCode * 397) ^ entity.Target.Path.GetHashCode();
                return hashCode;
            }
        }
    }
}