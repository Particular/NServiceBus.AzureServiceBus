using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration config, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topology = topology ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportConfig = config.UseTransport<AzureServiceBusTransport>();

        AzureServiceBusTransportConfigContext azureServiceBusTransportConfigContext;
        if (settings.TryGet("AzureServiceBus.AcceptanceTests.TransportConfigContext", out azureServiceBusTransportConfigContext))
        {
            azureServiceBusTransportConfigContext.Callback?.Invoke(endpointName, transportConfig);
        }
        else
        {
            transportConfig.ConnectionString(connectionString);
        }

        if (topology == "ForwardingTopology")
        {
            transportConfig.UseForwardingTopology();
        }
        else
        {
            var topologyConfig = transportConfig.UseEndpointOrientedTopology();
            foreach (var publisher in publisherMetadata.Publishers)
            {
                foreach (var eventType in publisher.Events)
                {
                    topologyConfig.RegisterPublisher(eventType, NameForEndpoint(publisher.PublisherType));
                }
            }
        }

        transportConfig.Sanitization()
            .UseStrategy<ValidateAndHashIfNeeded>();

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        config.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }

    // Copy of ATT Conventions.EndpointNamingConvention logic to convert nested endpoint class name to publisher name
    internal static string NameForEndpoint<T>() where T : class
    {
        return NameForEndpoint(typeof(T));
    }

    static string NameForEndpoint(Type endpointType)
    {
        var classAndEndpoint = endpointType.FullName.Split('.').Last();
        var testName = classAndEndpoint.Split('+').First();
        testName = testName.Replace("When_", "");
        var endpoint = classAndEndpoint.Split('+').Last();
        testName = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(testName);
        testName = testName.Replace("_", "");

        return testName + "." + endpoint;
    }
}