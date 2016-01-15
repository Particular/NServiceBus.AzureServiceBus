using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;


public class ConfigureInMemoryPersistence : IConfigureTestExecution
{
    public IEnumerable<Type> UnsupportedScenarioDescriptorTypes { get; } = new Type[0];

    public Task Configure(BusConfiguration configuration, IDictionary<string, string> settings)
    {
        configuration.UsePersistence<InMemoryPersistence>();
        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        // Nothing required for in-memory persistence
        return Task.FromResult(0);
    }
}