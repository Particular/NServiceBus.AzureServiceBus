namespace NServiceBus.Transport.AzureServiceBus
{
    class EmptySubscriptionFilter : IBrokerSideSubscriptionFilterInternal
    {
        public string Serialize()
        {
            return "1=1";
        }
    }
}