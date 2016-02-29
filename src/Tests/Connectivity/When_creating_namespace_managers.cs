namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_namespace_managers
    {
        [Test]
        public void Creates_new_namespace_managers()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value);

            var creator = new NamespaceManagerCreator(settings);

            var first = creator.Create("namespace");
            var second = creator.Create("namespace");

            Assert.IsInstanceOf<INamespaceManager>(first);
            Assert.IsInstanceOf<INamespaceManager>(second);
            Assert.AreNotEqual(first, second);
        }
    }
}