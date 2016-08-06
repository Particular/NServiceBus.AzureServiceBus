namespace NServiceBus.Transport.AzureServiceBus
{
    public interface ICreateNamespaceManagers
    {
        INamespaceManager Create(string namespaceName);
    }
}