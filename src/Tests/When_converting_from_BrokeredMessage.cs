namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("Azure")]
    public class When_converting_from_BrokeredMessage
    {
        [Test]
        public void Should_handle_missing_intent()
        {
            var brokeredMessage = new BrokeredMessage();
            
            // NOTE: At least property must be set for the brokered message converter to try and retrieve the intent
            brokeredMessage.Properties["SomeProperty"] = "SomeValue";
            
            Assert.DoesNotThrow(() => brokeredMessage.ToTransportMessage());
        }
    }
}
