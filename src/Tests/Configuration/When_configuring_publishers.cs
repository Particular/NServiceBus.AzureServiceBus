namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System.Collections.Generic;
    using System.Reflection;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_publishers
    {
        SettingsHolder settings;
        TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settings = new SettingsHolder();
            extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
        }

        [Test]
        public void Should_be_able_to_add_a_publisher_for_a_specific_event()
        {
            extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisher(typeof(MyType), "publisherName");

            var publishers = settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new SingleTypeScanner(typeof(MyType)));
        }

        [Test]
        public void Should_be_able_to_add_a_publisher_for_the_events_contained_in_an_assembly()
        {
            extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisher(Assembly.GetExecutingAssembly(), "publisherName");

            var publishers = settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new AssemblyTypesScanner(Assembly.GetExecutingAssembly()));
        }

        [Test, Explicit("Works as per design. It's impossible to access Publisher API using a topology other than `EndpointOrientedTopology`")]
        public void Should_not_be_possible_configure_publishers_using_forwarding_topology()
        {
//            var topologySettings = extensions.UseTopology<ForwardingTopology>();

//            Assert.Throws<InvalidOperationException>(() => topologySettings.RegisterPublisher("publisherName", typeof(MyType)));
//            Assert.Throws<InvalidOperationException>(() => topologySettings.RegisterPublisher("publisherName", Assembly.GetExecutingAssembly()));
        }

        class MyType
        {
        }
    }
}