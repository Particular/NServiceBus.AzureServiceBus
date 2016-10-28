namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Transport;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
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
