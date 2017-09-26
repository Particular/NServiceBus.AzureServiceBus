namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    interface ITopologySectionManagerInternal
    {
        TopologySectionInternal DetermineReceiveResources(string inputQueue);
        TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings, string localAddress);

        TopologySectionInternal DeterminePublishDestination(Type eventType, string localAddress);
        TopologySectionInternal DetermineSendDestination(string destination);

        TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType, string localAddress);
        TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype);
    }
}