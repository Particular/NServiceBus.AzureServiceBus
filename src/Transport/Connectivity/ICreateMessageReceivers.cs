namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface ICreateMessageReceiversInternal
    {
        Task<IMessageReceiverInternal> Create(string entityPath, string namespaceAlias);
    }
}