namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_namespace_managers
    {
        [Test]
        public void Creates_new_namespace_managers()
        {
            var creator = new NamespaceManagerCreator();

            var first = creator.Create(AzureServiceBusConnectionString.Value);
            var second = creator.Create(AzureServiceBusConnectionString.Value);

            Assert.IsInstanceOf<INamespaceManager>(first);
            Assert.IsInstanceOf<INamespaceManager>(second);
            Assert.AreNotEqual(first, second);
        }
    }
}