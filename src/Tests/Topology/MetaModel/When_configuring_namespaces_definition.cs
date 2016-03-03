namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespaces_definition
    {
        private NamespaceConfigurations _namespaces;

        [SetUp]
        public void SetUp()
        {
            _namespaces = new NamespaceConfigurations();
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Should_throws_an_exception_if_name_is_not_valid(string name)
        {
            var exception = Assert.Throws<ArgumentException>(() => _namespaces.Add(name, "connectionString"));
            Assert.AreEqual("name", exception.ParamName);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Should_throws_an_exception_if_connection_string_is_not_valid(string connectionString)
        {
            var exception = Assert.Throws<ArgumentException>(() => _namespaces.Add("name", connectionString));
            Assert.AreEqual("connectionString", exception.ParamName);
        }

        [Test]
        public void Should_add_definition_if_it_does_not_exist()
        {
            _namespaces.Add("name", "connectionString");

            Assert.AreEqual(1, _namespaces.Count);
        }

        [Test]
        public void Should_does_not_add_definition_if_exists()
        {
            _namespaces.Add("name", "connectionString");
            _namespaces.Add("Name", "connectionString");

            Assert.AreEqual(1, _namespaces.Count);
        }

        [Test]
        public void Should_add_default_connection_string_if_it_does_not_exist()
        {
            _namespaces.AddDefault("connectionString");

            Assert.AreEqual(1, _namespaces.Count);
        }

        [Test]
        public void Should_does_not_add_if_connection_string_exists()
        {
            _namespaces.Add("name", "connectionString");

            _namespaces.AddDefault("connectionString");

            Assert.AreEqual(1, _namespaces.Count);
        }

        [Test]
        public void Should_override_default_connection_string()
        {
            _namespaces.AddDefault("connectionString");

            _namespaces.Add("name", "connectionString");

            Assert.AreEqual(1, _namespaces.Count);
        }

        [Test]
        public void Should_get_connection_string_by_namespace_name_with_a_case_insensitive_match()
        {
            _namespaces.Add("name", "connectionString");

            var connectionString = _namespaces.GetConnectionString("NaMe");

            StringAssert.AreEqualIgnoringCase("connectionString", connectionString);
        }

        [Test]
        public void Should_throws_an_exception_if_namespace_name_has_not_been_registered()
        {
            Assert.Throws<KeyNotFoundException>(() => _namespaces.GetConnectionString("name"));
        }
    }
}