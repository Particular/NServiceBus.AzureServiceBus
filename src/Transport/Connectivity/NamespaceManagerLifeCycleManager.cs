namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;

    class NamespaceManagerLifeCycleManager : IManageNamespaceManagerLifeCycle
    {
        ICreateNamespaceManagersInternal factory;
        ConcurrentDictionary<string, INamespaceManagerInternal> namespaceManagers = new ConcurrentDictionary<string, INamespaceManagerInternal>();

        public NamespaceManagerLifeCycleManager(ICreateNamespaceManagersInternal factory)
        {
            this.factory = factory;
        }

        public INamespaceManagerInternal Get(string namespaceAlias)
        {
            return namespaceManagers.GetOrAdd(namespaceAlias, s => factory.Create(s));
        }

    }
}