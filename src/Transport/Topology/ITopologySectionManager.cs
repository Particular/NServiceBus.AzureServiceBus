namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Transport;

    interface ITopologySectionManagerInternal
    {
        TopologySection DetermineReceiveResources(string inputQueue);
        TopologySection DetermineResourcesToCreate(QueueBindings queueBindings);

        TopologySection DeterminePublishDestination(Type eventType);
        TopologySection DetermineSendDestination(string destination);

        TopologySection DetermineResourcesToSubscribeTo(Type eventType);
        TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype);
    }
}
