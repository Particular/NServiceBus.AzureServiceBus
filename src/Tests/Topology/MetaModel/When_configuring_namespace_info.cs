namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using Tests;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespace_info
    {
        [Test]
        [TestCase("name", "name")]
        [TestCase("name", "Name")]
        public void Two_namespaces_should_be_equal_if_alias_and_connection_string_are_case_insensitive_equal(string alias1, string alias2)
        {
            var namespaceInfo1 = new NamespaceInfo(alias1, ConnectionStringValue.Sample);
            var namespaceInfo2 = new NamespaceInfo(alias2, ConnectionStringValue.Sample);

            Assert.AreEqual(namespaceInfo1, namespaceInfo2);
        }

        [Test]
        [TestCase("name", "name")]
        [TestCase("name", "Name")]
        public void Two_namespaces_should_have_the_same_hash_code_if_alias_and_connection_string_are_case_insensitive_equal(string alias1, string alias2)
        {
            var namespaceInfo1 = new NamespaceInfo(alias1, ConnectionStringValue.Sample);
            var namespaceInfo2 = new NamespaceInfo(alias2, ConnectionStringValue.Sample);

            var hashCode1 = namespaceInfo1.GetHashCode();
            var hashCode2 = namespaceInfo2.GetHashCode();
            Assert.AreEqual(hashCode1, hashCode2);
        }
    }
}