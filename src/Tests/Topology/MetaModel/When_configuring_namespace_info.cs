namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using NServiceBus.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespace_info
    {
        [Test]
        [TestCase("name", "name")]
        [TestCase("name", "Name")]
        public void Two_namespaces_should_be_equal_if_name_and_connection_string_are_case_insensitive_equal(string name1, string name2)
        {
            var namespace1 = new NamespaceInfo(name1, ConnectionStringValue.Sample);
            var namespace2 = new NamespaceInfo(name2, ConnectionStringValue.Sample);

            Assert.AreEqual(namespace1, namespace2);
        }

        [Test]
        [TestCase("name", "name")]
        [TestCase("name", "Name")]
        public void Two_namespaces_should_have_the_same_hash_code_if_name_and_connection_string_are_case_insensitive_equal(string name1, string name2)
        {
            var namespace1 = new NamespaceInfo(name1, ConnectionStringValue.Sample);
            var namespace2 = new NamespaceInfo(name2, ConnectionStringValue.Sample);

            var hashCode1 = namespace1.GetHashCode();
            var hashCode2 = namespace2.GetHashCode();
            Assert.AreEqual(hashCode1, hashCode2);
        }
    }
}