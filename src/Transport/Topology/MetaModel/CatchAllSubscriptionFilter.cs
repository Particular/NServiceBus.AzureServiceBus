namespace NServiceBus.Transport.AzureServiceBus
{
    class CatchAllSubscriptionFilter : IBrokerSideSubscriptionFilter
    {
        public string Serialize()
        {
            return "1=1";
        }
    }
}