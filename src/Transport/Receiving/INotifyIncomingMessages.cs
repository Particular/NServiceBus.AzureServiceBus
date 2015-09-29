using System;

namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface INotifyIncomingMessages
    {
        bool IsRunning { get; }
        int RefCount { get; set; }

        void Initialize(string entitypath, string connectionstring, Func<IncomingMessage, ReceiveContext, Task> callback, Func<Exception, Task> errorCallback, int maximumConcurrency);

        Task Start();
        Task Stop();
    }
}