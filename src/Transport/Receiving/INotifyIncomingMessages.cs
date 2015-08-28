using System;

namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface INotifyIncomingMessages
    {
        void Initialize(Func<IncomingMessage, Task> callback);

        void Start();
        void Stop();
    }
}