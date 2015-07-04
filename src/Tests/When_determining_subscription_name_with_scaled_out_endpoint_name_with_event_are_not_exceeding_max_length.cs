namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name_with_scaled_out_endpoint_where_name_with_event_are_not_exceeding_max_length
    {
        [Test]
        public void Should_generate_a_guid_based_name_with_individualizer_appended()
        {
            var eventType = typeof(SomeEventWithAnInsanelyLongName);
            var endpointName = new string('a', 50 - eventType.Name.Length - 1 - 1); // 1 for "." and 1 for max length that is 49???
            var settings = new SettingsHolder();
            settings.Set("ScaleOut.UseSingleBrokerQueue", false);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(settings, eventType, endpointName);

            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionName.Replace("-" + Environment.MachineName, ""), out guid));
        }
    }
}