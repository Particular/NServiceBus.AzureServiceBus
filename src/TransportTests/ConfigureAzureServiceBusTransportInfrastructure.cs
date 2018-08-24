using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Serialization;
using NServiceBus.Settings;
using NServiceBus.Transport.AzureServiceBus;
using NServiceBus.TransportTests;

class ConfigureAzureServiceBusTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportConfigurationResult Configure(SettingsHolder settings, TransportTransactionMode transactionMode)
    {
        settings.Set("Transport.ConnectionString", Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString"));
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        settings.Set(new Conventions());
        settings.Set(WellKnownConfigurationKeys.Core.MainSerializerSettingsKey, Tuple.Create<SerializationDefinition, SettingsHolder>(new XmlSerializer(), settings));
        settings.Set("NServiceBus.SharedQueue", settings.Get("NServiceBus.Routing.EndpointName"));
        var topologyName = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topologyName = topologyName ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportExtension = new TransportExtensions<AzureServiceBusTransport>(settings);
        if (topologyName == "ForwardingTopology")
        {
            transportExtension.UseForwardingTopology();
        }
        else
        {
            transportExtension.UseEndpointOrientedTopology();
        }

        var transport = new AzureServiceBusTransport();
        var infrastructure = transport.Initialize(settings, connectionString);

        return new TransportConfigurationResult
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