using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Transports;

public class ConfigureAzureServiceBusTransport
{
    BusConfiguration busConfiguration;

    public Task Configure(BusConfiguration configuration)
    {
        busConfiguration = configuration;
        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        var bindings = busConfiguration.GetSettings().Get<QueueBindings>();
       
        return Task.FromResult(0);
    }
}