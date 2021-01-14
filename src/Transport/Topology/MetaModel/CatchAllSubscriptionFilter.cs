namespace NServiceBus.Transport.AzureServiceBus
{
    class CatchAllSubscriptionFilter : IBrokerSideSubscriptionFilterInternal
    {
        public string Serialize() => "1=1";
    }
}