namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System.Collections.Concurrent;
    using Microsoft.ServiceBus;
    using Support;

    class CreatesNamespaceManagers : ICreateNamespaceManagers
    {
        Configure config;

        ConcurrentDictionary<string, NamespaceManager> NamespaceManagers = new ConcurrentDictionary<string, NamespaceManager>();

        public CreatesNamespaceManagers(Configure config)
        {
            this.config = config;
        }

        public NamespaceManager Create(string potentialConnectionstring)
        {
            return NamespaceManagers.GetOrAdd(potentialConnectionstring, s =>
            {
                var connectionStringParser = new DeterminesBestConnectionStringForAzureServiceBus(config.TransportConnectionString());
                var connectionstring = s != RuntimeEnvironment.MachineName && connectionStringParser.IsPotentialServiceBusConnectionString(s)
                    ? s
                    : connectionStringParser.Determine(config.Settings);

               return NamespaceManager.CreateFromConnectionString(connectionstring);
            });
       
        }
    }
}