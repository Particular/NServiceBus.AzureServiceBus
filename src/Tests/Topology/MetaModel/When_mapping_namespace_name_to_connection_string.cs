namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_mapping_namespace_name_to_connection_string
    {
        private DefaultNamespaceNameToConnectionStringMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var namespaceConfigurations = new NamespaceConfigurations();
            namespaceConfigurations.Add("namespace1", "Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            namespaceConfigurations.Add("namespace2", "Endpoint=sb://namespace2.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            namespaceConfigurations.Add("namespace3", "Endpoint=sb://namespace3.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret");
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaceConfigurations);

            mapper = new DefaultNamespaceNameToConnectionStringMapper(settings);
        }

        [Test]
        public void Should_return_same_value_if_does_not_contain_namespace_name()
        {
            var mappedValue = mapper.Map("queuename");

            StringAssert.AreEqualIgnoringCase("queuename", mappedValue);
        }

        [Test]
        public void Should_throw_if_namespace_name_has_not_been_mapped()
        {
            Assert.Throws<KeyNotFoundException>(() => mapper.Map("queuename@namespace"));
        }

        [Test]
        public void Should_return_mapped_value_with_right_connection_string()
        {
            var mappedValue = mapper.Map("queuename@namespace1");

            StringAssert.AreEqualIgnoringCase("queuename@Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret", mappedValue);
        }
    }
}
