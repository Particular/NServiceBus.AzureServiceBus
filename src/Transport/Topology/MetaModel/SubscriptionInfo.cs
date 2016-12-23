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

        internal override bool DerivedEqual(EntityInfo entityInfo)
        {
            var other = entityInfo as SubscriptionInfo;
            if (other == null)
                return false;

            var entity = RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
            var otherEntity = other.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
            var targetPathEquals = string.Equals(entity.Target.Path, otherEntity.Target.Path);

            return string.Equals(Path, other.Path)          // subscriptoin name is matches
                   && Type == other.Type                    // both entities are subscriptions
                   && targetPathEquals                      // both target the same topic
                   && Equals(Namespace, other.Namespace);   // on the same namespace
        }
    }
}