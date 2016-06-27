namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_entity_address
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Should_throws_an_exception_if_value_is_empty(string value)
        {
            Assert.Throws<ArgumentException>(() => new EntityAddress(value));
        }

        [Test]
        [TestCase("EntityName")]
        [TestCase("EntityName@suffix")]
        public void Should_extract_name(string value)
        {
            var address = new EntityAddress(value);

            Assert.AreEqual("EntityName", address.Name);
        }

        [Test]
        public void Should_extract_suffix_if_exists()
        {
            var address = new EntityAddress("EntityName@suffix");

            Assert.AreEqual("suffix", address.Suffix);
            Assert.True(address.HasSuffix);
        }

        [Test]
        [TestCase("EntityName")]
        [TestCase("EntityName@")]
        public void Should_initialize_suffix_to_string_empty_if_does_not_exist(string value)
        {
            var address = new EntityAddress(value);

            Assert.AreEqual(string.Empty, address.Suffix);
        }

        [Test]
        [TestCase("EntityName@NoConnectionString", false)]
        [TestCase("EntityName@Endpoint=sb://namespace.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Secrets", true)]
        public void Should_initialize_has_connection_string(string value, bool hasConnectionString)
        {
            var address = new EntityAddress(value);

            Assert.AreEqual(hasConnectionString, address.HasConnectionString);
        }
    }
}
