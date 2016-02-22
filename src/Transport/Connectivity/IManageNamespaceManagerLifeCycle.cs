namespace NServiceBus.AzureServiceBus
{
    public interface IManageNamespaceManagerLifeCycle
    {
        INamespaceManager Get(string namespaceName);
    }
}