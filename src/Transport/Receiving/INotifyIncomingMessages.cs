namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Transports;

    public interface INotifyIncomingMessages
    {
        bool IsRunning { get; }
        int RefCount { get; set; }

        void Initialize(EntityInfo entity, Func<IncomingMessageDetails, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, Func<ErrorContext, Task<bool>> onRetryError, int maximumConcurrency);

        void Start();
        Task Stop();
    }
}