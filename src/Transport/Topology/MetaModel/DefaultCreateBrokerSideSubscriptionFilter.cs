namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using Connectivity;
    using System;
    using Transport.AzureServiceBus;

    class DefaultCreateBrokerSideSubscriptionFilter : ICreateBrokerSideSubscriptionFilter
    {
        public IBrokerSideSubscriptionFilter Create(Type type)
        {
            return new SqlSubscriptionFilter(type);
        }

        public IBrokerSideSubscriptionFilter CreateCatchAll()
        {
            return new CatchAllSubscriptionFilter();
        }
    }
}
