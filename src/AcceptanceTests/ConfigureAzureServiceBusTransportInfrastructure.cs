using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AzureServiceBus;
using NServiceBus.Settings;
using NServiceBus.Transport;
using NServiceBus.TransportTests;

class ConfigureAzureServiceBusTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportInfrastructure Configure(SettingsHolder settings)
    {
        settings.Set("Transport.ConnectionString", Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString"));
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        settings.Set<Conventions>(new Conventions());
        //settings.Set("NServiceBus.Routing.EndpointName", "onmessagethrowsafterdelayedretryreceiveonly");

        var endpointOrientedTopology = new EndpointOrientedTopology();
        //endpointOrientedTopology.Initialize(settings);
        settings.Set<ITopology>(endpointOrientedTopology);

        var transport = new AzureServiceBusTransport();
        return transport.Initialize(settings, connectionString);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}