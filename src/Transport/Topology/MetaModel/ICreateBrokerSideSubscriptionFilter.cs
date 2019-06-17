namespace NServiceBus.AzureServiceBus.Connectivity
{
    using System;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Contract to implement custom subscription filter <see cref="IBrokerSideSubscriptionFilter"/>.
    /// </summary>
    public interface ICreateBrokerSideSubscriptionFilter
    {
        /// <summary>
        /// Creates BrokerSideSubscriptionFilter for a given type
        /// </summary>
        IBrokerSideSubscriptionFilter Create(Type type);

        /// <summary>
        /// Creates BrokerSideSubscriptionFilter which produces 'catch-all' filter.
        /// </summary>
        IBrokerSideSubscriptionFilter CreateCatchAll();
    }
}
