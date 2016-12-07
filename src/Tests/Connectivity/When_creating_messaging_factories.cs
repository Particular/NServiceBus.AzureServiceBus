namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
    using AzureServiceBus;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

#pragma warning disable 618
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

            var lifecycleManager = new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)) , settings);

            var first = lifecycleManager.Create("namespace");
            var second = lifecycleManager.Create("namespace");

            Assert.IsInstanceOf<IMessagingFactoryInternal>(first);
            Assert.IsInstanceOf<IMessagingFactoryInternal>(second);
            Assert.AreNotEqual(first, second);
        }

    }
}