using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureAzureServiceBusTransport : IConfigureTestExecution
{
    public Task Configure(BusConfiguration config, IDictionary<string, string> settings)
    {
        config.UseTransport<AzureServiceBusTransport>()
            .ConnectionString(settings["Transport.ConnectionString"]);

        config.RegisterComponents(c =>
        {
            c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance);
        });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}
