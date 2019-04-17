namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;

    interface INotifyIncomingMessagesInternal
    {
        void Initialize(EntityInfoInternal entity, Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> callback, Func<Exception, Task> errorCallback, Action<string, Exception> raiseCriticalError, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency);

        void Start();
        Task Stop();
    }
}