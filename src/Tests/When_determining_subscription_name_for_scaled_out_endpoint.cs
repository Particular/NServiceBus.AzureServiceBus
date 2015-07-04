namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name_for_scaled_out_endpoint
    {
        [Test]
        public void Should_generate_a_guid_based_name_with_individualizer_suffix()
        {
            const string endpointName = "When_determining_subscription_name_for_scaled_out_endpoint";
            var eventType = typeof(SomeEventWithAnInsanelyLongName);
            var settings = new SettingsHolder();
            settings.Set("ScaleOut.UseSingleBrokerQueue", false);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(settings, eventType, endpointName);

            Assert.True(subscriptionName.Length <= 50);
            Assert.True(subscriptionName.EndsWith("-" + Environment.MachineName));

            Console.WriteLine(subscriptionName);
            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionName.Substring(0, 36), out guid));
        }
    }
}