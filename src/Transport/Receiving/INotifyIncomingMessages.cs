namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Transport;

    interface INotifyIncomingMessagesInternal
    {
        bool IsRunning { get; }
        int RefCount { get; set; }

        void Initialize(EntityInfoInternal entity, Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> callback, Func<Exception, Task> errorCallback, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency);

        void Start();
        Task Stop();
    }
}