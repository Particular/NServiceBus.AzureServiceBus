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

        var topologyName = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topologyName = topologyName ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        if (topologyName == "ForwardingTopology")
        {
#pragma warning disable 618
            settings.Set<ITopology>(new ForwardingTopology());
#pragma warning restore 618
        }
        else
        {
#pragma warning disable 618
            settings.Set<ITopology>(new EndpointOrientedTopology());
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