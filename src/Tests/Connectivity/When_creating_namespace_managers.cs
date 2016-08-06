namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
    using AzureServiceBus;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_namespace_managers
    {
        [Test]
        public void Creates_new_namespace_managers()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            var creator = new NamespaceManagerCreator(settings);

            var first = creator.Create("namespace");
            var second = creator.Create("namespace");

            Assert.IsInstanceOf<INamespaceManager>(first);
            Assert.IsInstanceOf<INamespaceManager>(second);
            Assert.AreNotEqual(first, second);
        }
    }
}