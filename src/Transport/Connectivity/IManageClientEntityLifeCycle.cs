namespace NServiceBus.AzureServiceBus
{
    public interface IManageClientEntityLifeCycle
    {
        IClientEntity Get(string entitypath, string connectionstring);
    }
}