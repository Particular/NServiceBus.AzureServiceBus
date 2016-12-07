namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface ICreateMessageSendersInternal
    {
        Task<IMessageSender> Create(string entitypath, string viaEntityPath, string namespaceName);
    }
}