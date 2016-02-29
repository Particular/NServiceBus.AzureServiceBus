using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
using NServiceBus.AzureServiceBus;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureScenariosForAzureServiceBusTransport : IConfigureSupportedScenariosForTestExecution
{
    public IEnumerable<Type> UnsupportedScenarioDescriptorTypes { get; } = new[]
    {
        typeof(AllDtcTransports),
        typeof(AllTransportsWithMessageDrivenPubSub),
        typeof(AllTransportsWithoutNativeDeferral),
        typeof(AllNativeMultiQueueTransactionTransports)
        //typeof(AllNativeMultiQueueTransactionTransports)
    };
}

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration config, RunSettings settings)
    {
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topology = topology ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportConfig = config.UseTransport<AzureServiceBusTransport>().ConnectionString(connectionString);

        if (topology == "ForwardingTopology")
        {
            transportConfig.UseTopology<ForwardingTopology>();
        }
        //else default

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}