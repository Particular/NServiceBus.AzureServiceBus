namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespace_configurations
    {
        private NamespaceConfigurations namespaces;

        [SetUp]
        public void SetUp()
        {
            namespaces = new NamespaceConfigurations();
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Should_throws_an_exception_if_name_is_not_valid(string name)
        {
            var exception = Assert.Throws<ArgumentException>(() => namespaces.Add(name, ConnectionStringValue.Sample));
            Assert.AreEqual("name", exception.ParamName);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Should_throws_an_exception_if_connection_string_is_not_valid(string connectionString)
        {
            var exception = Assert.Throws<ArgumentException>(() => namespaces.Add("name", connectionString));
            Assert.AreEqual("connectionString", exception.ParamName);
        }

        [Test]
        public void Should_add_definition_if_it_does_not_exist()
        {
            namespaces.Add("name", ConnectionStringValue.Sample);
            var connectionString = namespaces.GetConnectionString("name");

            Assert.AreEqual(1, namespaces.Count);
            Assert.AreEqual(ConnectionStringValue.Sample, connectionString);
        }

        [Test]
        public void Should_add_default_connection_string_if_it_does_not_exist()
        {
            namespaces.AddDefault(ConnectionStringValue.Sample);
            var connectionString = namespaces.GetConnectionString(NamespaceConfigurations.DefaultName);

            Assert.AreEqual(1, namespaces.Count);
            Assert.AreEqual(ConnectionStringValue.Sample, connectionString);
        }

        [Test]
        [TestCase("name", "name")]
        [TestCase("name", "Name")]
        [TestCase("name", "NAME")]
        public void Should_not_add_definition_if_namespace_name_already_exists_with_case_insensitive_check(string name1, string name2)
        {
            namespaces.Add(name1, ConnectionStringValue.Build("namespace1"));
            namespaces.Add(name2, ConnectionStringValue.Build("namespace2"));

            Assert.AreEqual(1, namespaces.Count);
        }

        [Test]
        public void Should_not_add_definition_if_connection_string_exists()
        {
            namespaces.Add("namespace1", ConnectionStringValue.Sample);
            namespaces.Add("namespace2", ConnectionStringValue.Sample);

            Assert.AreEqual(1, namespaces.Count);
        }

        [Test]
        public void Should_get_connection_string_by_namespace_name_with_a_case_insensitive_match()
        {
            namespaces.Add("name", ConnectionStringValue.Sample);

            var connectionString = namespaces.GetConnectionString("NaMe");

            StringAssert.AreEqualIgnoringCase(ConnectionStringValue.Sample, connectionString);
        }

        [Test]
        public void Should_throws_an_exception_if_namespace_name_has_not_been_registered()
        {
            Assert.Throws<KeyNotFoundException>(() => namespaces.GetConnectionString("name"));
        }
    }
}