namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Linq;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_ForwardingTopology
    {
        [Test]
        public void Determines_the_namespace_from_partitioning_strategy()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new ForwardingTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            var definition = topology.DetermineResourcesToCreate();

            var namespaceInfo = new NamespaceInfo(connectionstring, NamespaceMode.Active);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public void Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new ForwardingTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            var definition = topology.DetermineResourcesToCreate();

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == connectionstring));
        }

        [Test]
        public void Determines_there_should_be_a_topic_bundle_created()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            const string connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new ForwardingTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            var definition = topology.DetermineResourcesToCreate();

            var result = definition.Entities.Where(ei => ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == connectionstring && ei.Path.StartsWith("bundle-"));

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result, Has.Exactly(1).Matches<EntityInfo>(x => x.Path == "bundle-1"));
            Assert.That(result, Has.Exactly(1).Matches<EntityInfo>(x => x.Path == "bundle-2"));
        }

        [Test]
        public void Creates_subscription_on_each_topic_in_bundle()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            const string connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new ForwardingTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            topology.DetermineResourcesToCreate();

            var section = topology.DetermineResourcesToSubscribeTo(typeof(SomeEvent));

            Assert.That(section.Entities.Count(), Is.EqualTo(2));
            // TODO: need to verify that subscription is done on each topic
        }

        class SomeEvent
        {
        }
    }
}
