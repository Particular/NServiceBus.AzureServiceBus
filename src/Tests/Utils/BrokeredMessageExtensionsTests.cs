namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Utils
{
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    public class BrokeredMessageExtensionsTests
    {
        [Test]
        public void Estimate_should_return_size_if_available()
        {
            var message = new BrokeredMessage();
            message.Properties[BrokeredMessageHeaders.EstimatedMessageSize] = "100";

            Assert.AreEqual(100, message.EstimatedSize());
        }

        [Test]
        public void Estimate_without_header_present_should_return_size()
        {
            var message = new BrokeredMessage();

            Assert.AreEqual(0, message.EstimatedSize());
        }

        [Test]
        public void Estimate_with_null_header_present_should_return_size()
        {
            var message = new BrokeredMessage();
            message.Properties[BrokeredMessageHeaders.EstimatedMessageSize] = null;

            Assert.AreEqual(0, message.EstimatedSize());
        }
    }
}