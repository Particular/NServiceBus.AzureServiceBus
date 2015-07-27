namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus;

    class NamespaceManagerLifeCycleManager
    {
        ICreateNamespaceManagers _factory;
        ConcurrentDictionary<string, NamespaceManager> namespaceManagers = new ConcurrentDictionary<string, NamespaceManager>();

        public NamespaceManagerLifeCycleManager(ICreateNamespaceManagers factory)
        {
            this._factory = factory;
        }

        public NamespaceManager Get(string @namespace)
        {
            return namespaceManagers.GetOrAdd(@namespace, s => _factory.Create(s));
        }

    }
}