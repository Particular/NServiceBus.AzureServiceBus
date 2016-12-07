namespace NServiceBus.Transport.AzureServiceBus
{
    interface IManageNamespaceManagerLifeCycleInternal
    {
        INamespaceManagerInternal Get(string namespaceAlias);
    }
}