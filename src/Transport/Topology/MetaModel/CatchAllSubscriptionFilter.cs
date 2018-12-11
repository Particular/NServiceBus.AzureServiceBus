namespace NServiceBus.Transport.AzureServiceBus
{
    class CatchAllSubscriptionFilter : IBrokerSideSubscriptionFilterInternal
    {
        public string Serialize()
        {
            return "1=1";
        }
    }
}