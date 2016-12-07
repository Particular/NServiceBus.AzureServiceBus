namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface ICreateMessageSendersInternal
    {
        Task<IMessageSenderInternal> Create(string entitypath, string viaEntityPath, string namespaceName);
    }
}