namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name
    {
        [TestCase(49, false)]
        [TestCase(50, true)]
        public void Should_always_convert_name_to_guid_for_name_less_than_50_characters_despite_ASB_supporting_50(int endpointNameLength, bool shouldBeConvertedToGuid)
        {
            var endpointName = new string('x', endpointNameLength);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(null, null, endpointName);
            Guid result;
            Assert.AreEqual(shouldBeConvertedToGuid, Guid.TryParse(subscriptionName, out result));
        }
    }
}