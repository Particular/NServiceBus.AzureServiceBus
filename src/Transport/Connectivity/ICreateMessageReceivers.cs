namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface ICreateMessageReceivers
    {
        Task<IMessageReceiver> Create(string entitypath, string namespaceName);
    }
}