namespace NServiceBus.AcceptanceTests
{
    using NUnit.Framework;

    public static partial class Requires
    {
        public static void ForwardingTopology()
        {
            if (!TestSuiteConstraints.Current.IsForwardingTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the Forwarding topology to be configured.");
            }
        }

        public static void EndpointOrientedTopology()
        {
            if (!TestSuiteConstraints.Current.IsEndpointOrientedTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the Endpoint-Oriented topology to be configured.");
            }
        }
        public static void EndpointOrientedMigrationTopology()
        {
            if (!TestSuiteConstraints.Current.IsEndpointOrientedMigrationTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the Endpoint-Oriented Migration topology to be configured.");
            }
        }
    }
}