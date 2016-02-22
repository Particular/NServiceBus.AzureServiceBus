namespace NServiceBus.AzureServiceBus
{
    public interface ICreateNamespaceManagers
    {
        INamespaceManager Create(string namespaceName);
    }
}