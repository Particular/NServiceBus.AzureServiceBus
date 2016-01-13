namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_asking_for_brokered_message_total_estimated_size
    {
        [Test]
        public void Should_report_message_size_zero()
        {
            var message = new BrokeredMessage();

            Assert.That(message.Size, Is.EqualTo(0), "BrokeredMessage.Size issue has been fixed and this test fixture needs to be reevaluated");
        }

        Func<int, int> TenPercentOf = originalValue => (int) (originalValue * 1.1);
        Func<int, int> FiftennPercentOf = originalValue => (int) (originalValue * 1.15);

        [Test]
        public void Should_report_empty_message_size()
        {
            const int emptyMessageSize = 114;

            var message = new BrokeredMessage();
            var size = message.TotalEstimatedSize();

            Assert.That(size, Is.AtLeast(TenPercentOf(emptyMessageSize)).And.LessThan(FiftennPercentOf(emptyMessageSize)), "Empty message should still have a size");
        }

        [Test]
        public void Should_report_message_size_with_custom_properties()
        {
            const int emptyMessageSize = 114;
            const int propertyNameAndValueSize = 5 + 1024;

            var message = new BrokeredMessage();
            message.Properties["prop1"] = new string('A', 1024);
            var size = message.TotalEstimatedSize();

            Assert.That(size, Is.AtLeast(TenPercentOf(emptyMessageSize + propertyNameAndValueSize)).And.LessThan(FiftennPercentOf(emptyMessageSize + propertyNameAndValueSize)));
        }

        [Test]
        public void Should_report_message_size_with_body_as_byte_array()
        {
            const int emptyMessageSize = 114;
            const int bodySize = 1024*5;

            var message = new BrokeredMessage(Encoding.UTF8.GetBytes(new string('A', bodySize)));
            var size = message.TotalEstimatedSize();

            Assert.That(size, Is.AtLeast(TenPercentOf(emptyMessageSize + bodySize)).And.LessThan(FiftennPercentOf(emptyMessageSize + bodySize)));
        }

        [Test]
        public void Should_report_message_size_with_body_as_stream_and_custom_property()
        {
            const int emptyMessageSize = 114;
            const int bodySize = 1024*3;
            const int propertyNameAndValueSize = 4 + 1024;
            
            var message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(new string('A', bodySize))));
            message.Properties["prop"] = new string('A', 1024);
            var size = message.TotalEstimatedSize();

            Assert.That(size, Is.AtLeast(TenPercentOf(emptyMessageSize + bodySize + propertyNameAndValueSize)).And.LessThan(FiftennPercentOf(emptyMessageSize + bodySize + propertyNameAndValueSize)));
        }

        [Test]
        public void Should_report_message_size_with_body_as_byte_array_and_custom_property()
        {
            const int emptyMessageSize = 114;
            const int bodySize = 1024*3;
            const int propertyNameAndValueSize = 4 + 1024;
            
            var message = new BrokeredMessage(Encoding.UTF8.GetBytes(new string('A', bodySize)));
            message.Properties["prop"] = new string('A', 1024);
            var size = message.TotalEstimatedSize();

            Assert.That(size, Is.AtLeast(TenPercentOf(emptyMessageSize + bodySize + propertyNameAndValueSize)).And.LessThan(FiftennPercentOf(emptyMessageSize + bodySize + propertyNameAndValueSize)));
        }
    }
}