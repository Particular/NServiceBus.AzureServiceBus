namespace NServiceBus.AzureServiceBus
{
    using System;
  
    public interface ITopologySectionManager
    {

        TopologySection DetermineReceiveResources();
        TopologySection DetermineResourcesToCreate();

        TopologySection DeterminePublishDestination(Type eventType);
        TopologySection DetermineSendDestination(string destination);

        TopologySection DetermineResourcesToSubscribeTo(Type eventType);
        TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype);
        
    }
}
