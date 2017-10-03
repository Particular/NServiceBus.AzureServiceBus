namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;

    interface ITopologySectionManagerInternal
    {
        Func<Task> Initialize { get; set; } 

        TopologySectionInternal DetermineReceiveResources(string inputQueue);

        TopologySectionInternal DetermineTopicsToCreate(string localAddress);

        TopologySectionInternal DetermineQueuesToCreate(QueueBindings queueBindings, string localAddress);

        TopologySectionInternal DeterminePublishDestination(Type eventType, string localAddress);
        TopologySectionInternal DetermineSendDestination(string destination);

        TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType, string localAddress);
        TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype);
    }
}