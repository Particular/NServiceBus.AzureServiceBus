namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface ICreateMessageSenders
    {
        Task<IMessageSender> CreateAsync(string entitypath, string viaEntityPath, string connectionstring);
    }
}