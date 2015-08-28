using System;

namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface INotifyIncomingMessages
    {
        void Initialize(string entitypath, string connectionstring, Func<IncomingMessage, Task> callback, int maximumConcurrency);

        Task Start();
        Task Stop();
    }
}