namespace NServiceBus.AzureServiceBus
{
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
    }
}