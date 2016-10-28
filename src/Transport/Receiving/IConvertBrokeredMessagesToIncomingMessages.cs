namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertBrokeredMessagesToIncomingMessages
    {
        IncomingMessageDetails Convert(BrokeredMessage brokeredMessage);
    }
}