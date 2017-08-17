namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    interface ITopologySectionManagerInternal
    {
        TopologySectionInternal DetermineReceiveResources(string inputQueue);
        TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings);

        TopologySectionInternal DeterminePublishDestination(Type eventType);
        TopologySectionInternal DetermineSendDestination(string destination);

        TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType);
        TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype);
    }
}