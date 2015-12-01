namespace NServiceBus.AzureServiceBus.Tests.MetaModel
{
    using NUnit.Framework;
    using Test;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_SqlSubscriptionFilter_is_serializing_an_event
    {
        [Test]
        public void Should_create_a_valid_filtering_string()
        {
            var filter = new SqlSubscriptionFilter(typeof(SomeEvent));
            var result = filter.Serialize();
            const string expected = @"[NServiceBus.EnclosedMessageTypes] LIKE 'Test.SomeEvent%'"
                                   + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent%'" 
                                   + " OR [NServiceBus.EnclosedMessageTypes] LIKE '%Test.SomeEvent'" 
                                   + " OR [NServiceBus.EnclosedMessageTypes] = 'Test.SomeEvent'";

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}

namespace Test
{
    class SomeEvent
    {
    }
}