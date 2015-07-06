namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name_for_non_scaled_out_endpoint
    {
        [Test]
        public void Should_not_convert_name_to_guid_if_name_is_less_than_50_characters()
        {
            const string endpointName = "endpointname";
            var eventName = typeof(SomeEventWithAnInsanelyLongName);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(null, eventName, endpointName);

            Guid guid;
            Assert.IsFalse(Guid.TryParse(subscriptionName, out guid));
            Assert.AreEqual(endpointName + "." + eventName.Name, subscriptionName);
        }

        [Test]
        public void Should_replace_name_with_guid_subscription_name_over_50_characters()
        {
            const string endpointName = "Should_not_exceed_50_characters_and_replace_by_a_deterministic_guid";
            var subscriptionname = NamingConventions.SubscriptionNamingConvention(null, typeof(SomeEventWithAnInsanelyLongName), endpointName);

            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionname, out guid));
        }
    }


    public class SomeEventWithAnInsanelyLongName : IEvent
    {
    }
}