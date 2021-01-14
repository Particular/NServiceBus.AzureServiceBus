namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_mapping_connection_string_to_namespace_alias
    {
        DefaultConnectionStringToNamespaceAliasMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var namespaceConfigurations = new NamespaceConfigurations
            {
                { "alias1", "Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret", NamespacePurpose.Partitioning },
                { "alias2", "Endpoint=sb://namespace2.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret", NamespacePurpose.Partitioning },
                { "alias3", "Endpoint=sb://namespace3.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret", NamespacePurpose.Partitioning }
            };
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaceConfigurations);

            mapper = new DefaultConnectionStringToNamespaceAliasMapper(settings);
        }

        [Test]
        [TestCase("queuename")]
        [TestCase("queuename@notAConnectionString")]
        public void Should_return_same_value_if_does_not_contain_connection_string(string value)
        {
            var mappedValue = mapper.Map(new EntityAddress(value));

            StringAssert.AreEqualIgnoringCase(value, mappedValue.ToString());
        }

        [Test]
        public void Should_return_mapped_value_with_right_namespace_name()
        {
            var mappedValue = mapper.Map(new EntityAddress("queuename@Endpoint=sb://namespace1.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=secret"));

            StringAssert.AreEqualIgnoringCase("queuename@alias1", mappedValue.ToString());
        }
    }
}
