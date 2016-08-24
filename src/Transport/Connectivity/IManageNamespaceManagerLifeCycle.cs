namespace NServiceBus.Transport.AzureServiceBus
{
    public interface IManageNamespaceManagerLifeCycle
    {
        INamespaceManager Get(string namespaceAlias);
    }
}