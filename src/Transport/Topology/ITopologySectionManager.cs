namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Transport;

    interface ITopologySectionManagerInternal
    {
        TopologySectionInternal DetermineReceiveResources(string inputQueue);
        Task<TopologySectionInternal> DetermineResourcesToCreate(QueueBindings queueBindings);

        Task<TopologySectionInternal> DeterminePublishDestination(Type eventType);
        TopologySectionInternal DetermineSendDestination(string destination);

        Task<TopologySectionInternal> DetermineResourcesToSubscribeTo(Type eventType);
        TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventType);
    }
}
