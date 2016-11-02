using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Settings;
using NServiceBus.Transport.AzureServiceBus;
using NServiceBus.TransportTests;

class ConfigureAzureServiceBusTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportConfigurationResult Configure(SettingsHolder settings, TransportTransactionMode transactionMode)
    {
        settings.Set("Transport.ConnectionString", Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString"));
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        settings.Set<Conventions>(new Conventions());
        //settings.Set("NServiceBus.Routing.EndpointName", "onmessagethrowsafterdelayedretryreceiveonly");

        var topologyName = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topologyName = topologyName ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        //endpointOrientedTopology.Initialize(settings);
        if (topologyName == nameof(ForwardingTopology))
        {
            var topology = Activator.CreateInstance<ForwardingTopology>();
#pragma warning disable 618
            settings.Set<ITopology>(topology);
#pragma warning restore 618
        }
        else
        {
            var topology = Activator.CreateInstance<EndpointOrientedTopology>();
#pragma warning disable 618
            settings.Set<ITopology>(topology);
#pragma warning restore 618
        }

        var transport = new AzureServiceBusTransport();
        var infrastructure = transport.Initialize(settings, connectionString);

        return new TransportConfigurationResult()
        {
            PurgeInputQueueOnStartup = false,
            TransportInfrastructure = infrastructure
        };
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
    
}