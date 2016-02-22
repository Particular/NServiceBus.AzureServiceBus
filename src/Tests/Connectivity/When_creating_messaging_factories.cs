namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_messaging_factories
    {
        [Test]
        public void Creates_new_factories_for_namespace()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespacesDefinition>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.AddDefault(AzureServiceBusConnectionString.Value);

            var lifecycleManager = new MessagingFactoryCreator(new NamespaceManagerLifeCycleManager(new NamespaceManagerCreator(settings)) , settings);

            var first = lifecycleManager.Create("default");
            var second = lifecycleManager.Create("default");

            Assert.IsInstanceOf<IMessagingFactory>(first);
            Assert.IsInstanceOf<IMessagingFactory>(second);
            Assert.AreNotEqual(first, second);
        }

    }
}