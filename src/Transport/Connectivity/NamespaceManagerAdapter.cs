namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;

    class NamespaceManagerAdapter : INamespaceManager
    {
        readonly NamespaceManager _manager;

        public NamespaceManagerAdapter(NamespaceManager manager)
        {
            _manager = manager;
        }

        public NamespaceManagerSettings Settings
        {
            get { return _manager.Settings; }
        }

        public Uri Address
        {
            get { return _manager.Address; }
        }
    }
}