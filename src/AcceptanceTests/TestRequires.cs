namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests
{
    using NServiceBus.AcceptanceTests;
    using NUnit.Framework;

    // Core's Requires is not a partial class
    public class TestRequires
    {
        public static void ForwardingTopology()
        {
            if (!TestSuiteConstraints.Current.IsForwardingTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the forwarding topology to be configured.");
            }
        }

        public static void EndpointOrientedTopology()
        {
            if (!TestSuiteConstraints.Current.IsEndpointOrientedTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the endpoint oriented topology to be configured.");
            }
        }
    }
}