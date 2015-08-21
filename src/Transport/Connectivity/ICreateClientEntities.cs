namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface ICreateClientEntities
    {
        Task<IClientEntity> CreateAsync(string entitypath, string connectionstring);
    }
}