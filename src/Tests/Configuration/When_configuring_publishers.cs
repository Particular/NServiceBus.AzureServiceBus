namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_publishers
    {
        [Test]
        public void Should_be_able_to_add_a_new_publisher_for_a_specific_event()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<EndpointOrientedTopology>().RegisterPublisherFor<MyEvent>("publisher");

            var publishers = settings.Get<IDictionary<Type, List<string>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey(typeof(MyEvent)));
            CollectionAssert.Contains(publishers[typeof(MyEvent)], "publisher");

        }

        [Test]
        public void Should_be_able_to_add_two_publishers_for_the_same_message_type()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherFor<MyEvent>("publisher1")
                .RegisterPublisherFor<MyEvent>("publisher2");

            var publishers = settings.Get<IDictionary<Type, List<string>>>(WellKnownConfigurationKeys.Topology.Publishers);

            Assert.True(publishers.ContainsKey(typeof(MyEvent)));
            CollectionAssert.Contains(publishers[typeof(MyEvent)], "publisher1");
            CollectionAssert.Contains(publishers[typeof(MyEvent)], "publisher2");
        }

        public class MyEvent : IEvent
        {
        }
    }
}