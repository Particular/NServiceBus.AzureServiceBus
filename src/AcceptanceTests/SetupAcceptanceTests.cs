using NServiceBus.AzureServiceBus.AcceptanceTests;
using NUnit.Framework;

/// <summary>
/// Global setup fixture
/// </summary>
[SetUpFixture]
public class SetupAcceptanceTests
{
    [SetUp]
    public void SetUp()
    {
        // case 1:
        // ConfigureTopology = (trans) => (trans as ASB).Topology = Activator.Create(env["topology"]) // for "metadata" about the transport
        // case 2:
        // setting topology for all ATTs
        // setting.Set<TransportDefinition>(topology_to_use)

        //fix time
        var timeSync = new TimeSynchronization();
        timeSync.Sync();
    }
}

