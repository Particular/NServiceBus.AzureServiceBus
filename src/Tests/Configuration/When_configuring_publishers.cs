namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NServiceBus.AzureServiceBus.TypesScanner;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_publishers
    {
        private SettingsHolder _settings;
        private TransportExtensions<AzureServiceBusTransport> _extensions;

        [SetUp]
        public void SetUp()
        {
            _settings = new SettingsHolder();
            _extensions = new TransportExtensions<AzureServiceBusTransport>(_settings);
        }

        [Test]
        public void Should_be_able_to_add_a_publisher_for_a_specific_event()
        {
            _extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForType("publisherName", typeof(MyType));

            var publishers = _settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new SingleTypeScanner(typeof(MyType)));
        }

        [Test]
        public void Should_be_able_to_add_a_publisher_for_the_events_contained_in_an_assembly()
        {
            _extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForAssembly("publisherName", Assembly.GetExecutingAssembly());

            var publishers = _settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new AssemblyTypesScanner(Assembly.GetExecutingAssembly()));
        }

        [Test]
        public void Should_not_be_possible_configure_publishers_using_forwarding_topology()
        {
            var topologySettings = _extensions.UseTopology<ForwardingTopology>();

            Assert.Throws<InvalidOperationException>(() => topologySettings.RegisterPublisherForType("publisherName", typeof(MyType)));
            Assert.Throws<InvalidOperationException>(() => topologySettings.RegisterPublisherForAssembly("publisherName", Assembly.GetExecutingAssembly()));
        }

        class MyType
        {
        }
    }
}