namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using NServiceBus.AzureServiceBus.TypesScanner;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_publishers
    {
        [Test]
        public void Should_be_able_to_add_a_publisher_for_a_specific_event()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForType("publisherName", typeof(MyType));

            var publishers = settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new SingleTypeScanner(typeof(MyType)));
        }
        
        [Test]
        public void Should_be_able_to_add_a_publisher_for_the_events_contained_in_an_assembly()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForAssembly("publisherName", Assembly.GetExecutingAssembly());

            var publishers = settings.Get<IDictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey("publisherName"));
            CollectionAssert.Contains(publishers["publisherName"], new AssemblyTypesScanner(Assembly.GetExecutingAssembly()));
        }

        class MyType 
        {
        }
    }
}