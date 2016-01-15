//using NServiceBus.AzureServiceBus.AcceptanceTests;

using System;
using System.Reflection;
using NServiceBus;
using NServiceBus.AzureServiceBus;
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
        //// hack to make the ATT's scenario descriptors pick the correct tests
        //var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        //var field = typeof(AzureServiceBusTransport).GetField("_fallbackForScenarioDescriptors", BindingFlags.NonPublic | BindingFlags.Static);
        //if (topology == "ForwardingTopology")
        //{
        //    field.SetValue(null, (Func<ITopology>)(() => new ForwardingTopology()), BindingFlags.NonPublic | BindingFlags.Static, null, null);
        //}
        //else
        //{
        //    field.SetValue(null, (Func<ITopology>)(() => new StandardTopology()), BindingFlags.NonPublic | BindingFlags.Static, null, null);
        //}
    }
}

