namespace NServiceBus.AcceptanceTests
{
    using NUnit.Framework;

    public static partial class Requires
    {
        public static void ForwardingToplogy()
        {
            if (!TestSuiteConstraints.Current.IsForwardingTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the forwarding topology to be configured.");
            }
        }

        public static void EndpointOrientedToplogy()
        {
            if (!TestSuiteConstraints.Current.IsEndpointOrientedTopology)
            {
                Assert.Ignore("Ignoring this test because it requires the endpoint oriented topology to be configured.");
            }
        }
    }
}