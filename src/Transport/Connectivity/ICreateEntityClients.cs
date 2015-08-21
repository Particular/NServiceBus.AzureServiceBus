namespace NServiceBus.AzureServiceBus
{
    public interface ICreateEntityClients
    {
        IEntityClient Create(string entitypath);
    }
}