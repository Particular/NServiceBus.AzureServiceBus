namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_SqlSubscriptionFilter_is_serializing_an_event
    {
        [Test]
        public void Should_create_a_valid_filtering_string()
        {
            var filter = new SqlSubscriptionFilter(typeof(SomeEvent));
            var result = filter.Serialize();
            const string expected = @"[NServiceBus.EnclosedMessageTypes] LIKE '%NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel.SomeEvent%'";

            Assert.That(result, Is.EqualTo(expected));
        }
    }

    class SomeEvent
    {
    }
}