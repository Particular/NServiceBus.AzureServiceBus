namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System.Linq;
    using AzureServiceBus.Topology.MetaModel;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]

    public class When_working_with_satellite_transport_address_collection
    {
        [Test]
        public void Should_register_transport_address_only_once()
        {
            var collection = new SatelliteTransportAddressCollection();
            collection.Add(".retries");
            collection.Add(".RETRIES");

            Assert.That(collection.Count(), Is.EqualTo(1));
        }

        [TestCase(".retries")]
        [TestCase(".Retries")]
        [TestCase(".RETRIES")]
        public void Should_determine_if_transport_address_is_contained_in_case_insensitive_manner(string transportAddress)
        {
            var collection = new SatelliteTransportAddressCollection();
            collection.Add(".retries");

            Assert.IsTrue(collection.Contains(transportAddress));
        }
    }
}