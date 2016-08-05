namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_message_senders
    {
        [Test]
        public void Should_be_able_to_set_retrypolicy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageSenders().RetryPolicy(RetryPolicy.NoRetry);

            Assert.IsInstanceOf<NoRetry>(connectivitySettings.GetSettings().Get<RetryPolicy>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryPolicy));
        }

        [Test]
        public void Should_be_able_to_set_backoff_time_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageSenders().BackOffTimeOnThrottle(TimeSpan.FromSeconds(20));

            Assert.AreEqual(TimeSpan.FromSeconds(20), connectivitySettings.GetSettings().Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageSenders.BackOffTimeOnThrottle));
        }

        [Test]
        public void Should_be_not_able_to_set_invalid_backoff_time_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Assert.Throws<ArgumentOutOfRangeException>(() => extensions.MessageSenders().BackOffTimeOnThrottle(TimeSpan.FromSeconds(-1)));
        }

        [Test]
        public void Should_be_able_to_set_retry_attempts_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageSenders().RetryAttemptsOnThrottle(10);

            Assert.AreEqual(10, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.RetryAttemptsOnThrottle));
        }

        [Test]
        public void Should_be_able_to_set_invalid_retry_attempts_when_throttled()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Assert.Throws<ArgumentOutOfRangeException>(() => extensions.MessageSenders().RetryAttemptsOnThrottle(-1));
        }

        [Test]
        public void Should_be_able_to_set_maximum_message_size_in_kilobytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageSenders().MaximuMessageSizeInKilobytes(200);

            Assert.AreEqual(200, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MaximumMessageSizeInKilobytes));
        }

        [Test]
        public void Should_not_be_able_to_set_invalid_maximum_message_size_in_kilobytes()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Assert.Throws<ArgumentOutOfRangeException>(() => extensions.MessageSenders().MaximuMessageSizeInKilobytes(0));
        }

        [Test]
        public void Should_be_able_to_set_message_size_padding_percentage()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var connectivitySettings = extensions.MessageSenders().MessageSizePaddingPercentage(10);

            Assert.AreEqual(10, connectivitySettings.GetSettings().Get<int>(WellKnownConfigurationKeys.Connectivity.MessageSenders.MessageSizePaddingPercentage));
        }

        [Test]
        public void Should_be_not_be_able_to_set_invalid_message_size_padding_percentage()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            Assert.Throws<ArgumentOutOfRangeException>(() => extensions.MessageSenders().MessageSizePaddingPercentage(-1));
        }

        [Test]
        public void Should_be_able_to_set_oversized_brokered_message_handler()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var myOversizedBrokeredMessageHandler = new MyOversizedBrokeredMessageHandler();
            var connectivitySettings = extensions.MessageSenders().OversizedBrokeredMessageHandler(myOversizedBrokeredMessageHandler);

            Assert.AreEqual(myOversizedBrokeredMessageHandler, connectivitySettings.GetSettings().Get<IHandleOversizedBrokeredMessages>(WellKnownConfigurationKeys.Connectivity.MessageSenders.OversizedBrokeredMessageHandlerInstance));
        }

        class MyOversizedBrokeredMessageHandler : IHandleOversizedBrokeredMessages
        {
            public Task Handle(BrokeredMessage brokeredMessage)
            {
                return TaskEx.Completed;
            }
        }
    }

}