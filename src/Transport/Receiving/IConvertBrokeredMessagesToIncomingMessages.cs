namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertBrokeredMessagesToIncomingMessages
    {
        IncomingMessageDetails Convert(BrokeredMessage brokeredMessage);
    }
}