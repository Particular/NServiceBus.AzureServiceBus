namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface ICreateMessageReceivers
    {
        Task<IMessageReceiver> CreateAsync(string entitypath, string connectionstring);
    }
}