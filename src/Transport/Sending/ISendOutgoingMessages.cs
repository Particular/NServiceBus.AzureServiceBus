namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface ISendOutgoingMessages
    {
        Task SendAsync(OutgoingMessage message, DispatchOptions dispatchOptions);
    }
}