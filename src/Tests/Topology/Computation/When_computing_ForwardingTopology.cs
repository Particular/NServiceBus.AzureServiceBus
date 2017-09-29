namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Computation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using NUnit.Framework;
    using Transport;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_ForwardingTopology
    {
        const string Connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        const string Name = "name";

        [Test]
        public async Task Determines_the_namespace_from_partitioning_strategy()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            var definition = sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            // ReSharper disable once RedundantArgumentDefaultValue
            var namespaceInfo = new RuntimeNamespaceInfo(Name, Connectionstring);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public async Task Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            var definition = sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == Connectionstring));
        }

        [TestCase("Path is too long", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [TestCase("Path contains invalid character", "input%queue")]
        public async Task Should_fail_sanitization_for_invalid_endpoint_name(string reasonToFail, string endpointName)
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            Assert.Throws<Exception>(() => sectionManager.DetermineResourcesToCreate(new QueueBindings(), endpointName), "Was expected to fail: " + reasonToFail);
        }

        [Test]
        public async Task Determines_there_should_be_a_topic_bundle_created()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            var definition = sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            var result = definition.Entities.Where(ei => ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == Connectionstring && ei.Path.StartsWith("bundle-"));

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result, Has.Exactly(1).Matches<EntityInfoInternal>(x => x.Path == "bundle-1"));
        }

        [Test]
        public async Task Creates_subscription_for_topic_in_bundle()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.UseForwardingTopology().NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            var section = sectionManager.DetermineResourcesToSubscribeTo(typeof(SomeTestEvent), "sales");

            Assert.That(section.Entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Creates_subscription_path_matching_the_subscribing_endpoint_name()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.UseForwardingTopology().NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            var section = sectionManager.DetermineResourcesToSubscribeTo(typeof(SomeTestEvent), "sales");

            Assert.IsTrue(section.Entities.All(e => e.Path == "sales"), "Subscription name should be matching subscribing endpoint name, but it wasn't.");
        }

        [Test]
        public async Task Should_creates_subscription_entities_marked_as_not_be_listened_to()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.UseForwardingTopology().NamespacePartitioning().AddNamespace(Name, Connectionstring);

            var topology = new ForwardingTransportInfrastructure(settings);
            await topology.Start();

            var sectionManager = topology.topologyManager;
            sectionManager.DetermineResourcesToCreate(new QueueBindings(), "sales");

            var section = sectionManager.DetermineResourcesToSubscribeTo(typeof(SomeTestEvent), "sales");

            Assert.IsFalse(section.Entities.Any(x => x.ShouldBeListenedTo));
        }
    }
}

class SomeTestEvent
{
}
