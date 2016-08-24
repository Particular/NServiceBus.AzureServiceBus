namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;

    class NamespaceManagerLifeCycleManager : IManageNamespaceManagerLifeCycle
    {
        ICreateNamespaceManagers factory;
        ConcurrentDictionary<string, INamespaceManager> namespaceManagers = new ConcurrentDictionary<string, INamespaceManager>();

        public NamespaceManagerLifeCycleManager(ICreateNamespaceManagers factory)
        {
            this.factory = factory;
        }

        public INamespaceManager Get(string namespaceAlias)
        {
            return namespaceManagers.GetOrAdd(namespaceAlias, s => factory.Create(s));
        }

    }
}