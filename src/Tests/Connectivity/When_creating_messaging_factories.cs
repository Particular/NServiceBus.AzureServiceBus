namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
    using AzureServiceBus;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_messaging_factories
    {
        [Test]
        public void Creates_new_factories_for_namespace()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            var lifecycleManager = new MessagingFactoryCreator(new NamespaceManagerLifeCycleManager(new NamespaceManagerCreator(settings)) , settings);

            var first = lifecycleManager.Create("namespace");
            var second = lifecycleManager.Create("namespace");

            Assert.IsInstanceOf<IMessagingFactory>(first);
            Assert.IsInstanceOf<IMessagingFactory>(second);
            Assert.AreNotEqual(first, second);
        }

    }
}