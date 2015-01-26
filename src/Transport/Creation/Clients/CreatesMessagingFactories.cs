namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    class CreatesMessagingFactories : ICreateMessagingFactories
    {
        ICreateNamespaceManagers createNamespaceManagers;

        public CreatesMessagingFactories(ICreateNamespaceManagers createNamespaceManagers)
        {
            this.createNamespaceManagers = createNamespaceManagers;
        }

        public MessagingFactory Create(Address address)
        {
            var potentialConnectionString = address.Machine;
            var namespaceManager = createNamespaceManagers.Create(potentialConnectionString);

            var settings = new MessagingFactorySettings
            {
                TokenProvider = namespaceManager.Settings.TokenProvider,
                NetMessagingTransportSettings =
                {
                    BatchFlushInterval = TimeSpan.FromSeconds(0.1)
                }
            };
            return MessagingFactory.Create(namespaceManager.Address, settings);
        }
    }
}