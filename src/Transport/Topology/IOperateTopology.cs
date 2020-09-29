namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Operational aspects of running on top of the topology
    /// Takes care of the topology and it's specific state at runtime
    /// Examples
    /// Decisions of currently active namespace go here f.e.
    /// So is the list of notifiers etc...
    /// etc..
    /// </summary>
    interface IOperateTopologyInternal
    {
        //invoked for static parts of the topology
        void Start(TopologySectionInternal topology, int maximumConcurrency);
        Task Stop();

        //invoked whenever subscriptions are added or removed
        void Start(IEnumerable<EntityInfoInternal> subscriptions);
        Task Stop(IEnumerable<EntityInfoInternal> subscriptions);

        // callback when there is a new message available, or an error occurs
        void OnIncomingMessage(Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> func);

        void OnError(Func<Exception, Task> func);
        void SetCriticalError(CriticalError criticalError);

        void OnProcessingFailure(Func<ErrorContext, Task<ErrorHandleResult>> onError);
    }
}