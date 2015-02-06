namespace NServiceBus.Azure.QuickTests
{
    using NUnit.Framework;

    [TestFixture]
    [Category("Azure")]
    public class When_parsing_connectionstrings
    {
        [Test]
        public void Should_parse_queuename_from_azure_servicebus_connectionstring()
        {
            const string connectionstring = "myqueue@Endpoint=sb://nservicebus.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=w8EkqRS8y6ddYcVu75LPHfTeJIXm21Yu3XJiRxA3LOc=";
            
            var queueName = new Transports.WindowsAzureServiceBus.ConnectionStringParser().ParseQueueNameFrom(connectionstring);

            Assert.AreEqual(queueName, "myqueue");
        }

        [Test]
        public void Should_parse_namespace_from_azure_servicebus_connectionstring()
        {
            const string connectionstring = "myqueue@Endpoint=sb://nservicebus.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=w8EkqRS8y6ddYcVu75LPHfTeJIXm21Yu3XJiRxA3LOc=";

            var @namespace = new Transports.WindowsAzureServiceBus.ConnectionStringParser().ParseNamespaceFrom(connectionstring);

            Assert.AreEqual(@namespace, "Endpoint=sb://nservicebus.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=w8EkqRS8y6ddYcVu75LPHfTeJIXm21Yu3XJiRxA3LOc=");
        }

        [Test]
        public void Should_parse_queueindex_from_queuename_using_dots() // dots are allowed in azure servicebus queuenames
        {
            const string connectionstring = "myqueue.1";

            var index = new Transports.WindowsAzureServiceBus.ConnectionStringParser().ParseIndexFrom(connectionstring);

            Assert.AreEqual(index, 1);
        }
    }
}