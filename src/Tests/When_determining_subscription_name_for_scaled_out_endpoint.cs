namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NServiceBus.Settings;
    using NServiceBus.Support;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_subscription_name_for_scaled_out_endpoint
    {
        [Test]
        public void Should_generate_a_guid_based_name_with_individualizer_suffix_for_machine_name_under_14_characters()
        {
            RuntimeEnvironment.MachineNameAction = () => "DEV";
            const string endpointName = "When_determining_subscription_name_for_scaled_out_endpoint";
            var eventType = typeof(SomeEventWithAnInsanelyLongName);
            var settings = new SettingsHolder();
            settings.Set("ScaleOut.UseSingleBrokerQueue", false);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(settings, eventType, endpointName);

            Assert.True(subscriptionName.Length <= 50);
            Assert.True(subscriptionName.EndsWith("-" + RuntimeEnvironment.MachineName), "expected subscription name to end with machine name, but it didn't. Subscription name: " + subscriptionName + " machine name: " + Environment.MachineName);

            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionName.Substring(0, 36), out guid), "expected to have a guid, but got: " + subscriptionName);
        }

        [Test]
        public void Should_generate_a_guid_only_based_name_for_machine_name_over_14_characters()
        {
            RuntimeEnvironment.MachineNameAction = () => "DEVMACHINENAME";
            const string endpointName = "When_determining_subscription_name_for_scaled_out_endpoint";
            var eventType = typeof(SomeEventWithAnInsanelyLongName);
            var settings = new SettingsHolder();
            settings.Set("ScaleOut.UseSingleBrokerQueue", false);
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(settings, eventType, endpointName);

            Guid guid;
            Assert.IsTrue(Guid.TryParse(subscriptionName, out guid), "expected to have a guid, but got: " + subscriptionName);
        }
    }
}