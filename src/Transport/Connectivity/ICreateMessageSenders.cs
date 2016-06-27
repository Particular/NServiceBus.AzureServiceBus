namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface ICreateMessageSenders
    {
        Task<IMessageSender> Create(string entitypath, string viaEntityPath, string namespaceName);
    }
}