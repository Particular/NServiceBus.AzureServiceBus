namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageSender : IClientEntity
    {
        Task Send(BrokeredMessage message);

        Task SendBatch(IEnumerable<BrokeredMessage> messages);
    }
}