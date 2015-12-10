using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
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
    }
}

