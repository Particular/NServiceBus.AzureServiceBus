namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Concurrent;

    class NamespaceManagerLifeCycleManager : IManageNamespaceManagerLifeCycle
    {
        ICreateNamespaceManagers _factory;
        ConcurrentDictionary<string, INamespaceManager> namespaceManagers = new ConcurrentDictionary<string, INamespaceManager>();

        public NamespaceManagerLifeCycleManager(ICreateNamespaceManagers factory)
        {
            this._factory = factory;
        }

        public INamespaceManager Get(string @namespace)
        {
            return namespaceManagers.GetOrAdd(@namespace, s => _factory.Create(s));
        }

    }
}