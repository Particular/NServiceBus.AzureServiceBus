namespace NServiceBus.AzureServiceBus
{
    using System;
    using Transports;

    public interface ITopologySectionManager
    {

        TopologySection DetermineReceiveResources(string inputQueue);
        TopologySection DetermineResourcesToCreate(QueueBindings queueBindings);

        TopologySection DeterminePublishDestination(Type eventType);
        TopologySection DetermineSendDestination(string destination);

        TopologySection DetermineResourcesToSubscribeTo(Type eventType);
        TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype);
    }
}
