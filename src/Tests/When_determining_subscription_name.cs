namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name
    {
        [Test]
        public void Should_replace_name_with_guid_subscription_name_over_50_characters()
        {
            var subscriptionname = NamingConventions.SubscriptionNamingConvention(null, typeof(SomeEventWithAnInsanelyLongName), "Should_not_exceed_50_characters_and_replace_by_a_deterministic_guid");

            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionname, out guid));
        }
    }


    public class SomeEventWithAnInsanelyLongName : IEvent
    {
    }
}