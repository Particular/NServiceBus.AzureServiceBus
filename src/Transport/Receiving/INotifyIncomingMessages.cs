namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Transport;

    [Obsolete("Internal contract that shouldn't be exposed. Will be treated as an error from version 8.0.0. Will be removed in version 9.0.0.", false)]
    public interface INotifyIncomingMessages
    {
        bool IsRunning { get; }
        int RefCount { get; set; }

        void Initialize(EntityInfo entity, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, Func<ErrorContext, Task<ErrorHandleResult>> processingFailureCallback, int maximumConcurrency);

        void Start();
        Task Stop();
    }
}