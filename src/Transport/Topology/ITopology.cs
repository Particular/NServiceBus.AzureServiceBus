namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    // a topology is responsible to determine what the underlying physical topology in asb looks like
    // This includes, which namespaces are to be used
    // which shared resources must exist (like audit & error queue, or shared topics, eventhubs, etc...)
    // which endpoint specific resources (like input queue)
    // how these entities relate (forwarding f.e.)

    // internally it relies on addressing strategy and friends, but ultimately it provides a topology definition

    public interface ITopology
    {
        /// <summary>
        /// Properly initializes configuration, called while settings can still be changed
        /// </summary>
        void InitializeSettings();

        /// <summary>
        /// Properly sets up the container, called when settings are set
        /// </summary>
        void InitializeContainer();

        /// <summary>
        /// Creates the topology definition, called when settings are set
        /// </summary>
        TopologyDefinition Determine(Purpose purpose);
        TopologyDefinition Determine(Purpose sending, Type eventType);
        TopologyDefinition Determine(Purpose sending, string destination);

        IEnumerable<SubscriptionInfo> Subscribe(Type eventType);
        IEnumerable<SubscriptionInfo> Unsubscribe(Type eventtype);
        
    }
}
