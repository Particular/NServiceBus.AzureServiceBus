namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateMessageReceivers
    {
        MessageReceiver Create(string entitypath);
    }
}