namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_namespace_managers
    {
        [Test]
        public void Creates_new_namespace_manager()
        {
            var creator = new NamespaceManagerCreator();

            var manager = creator.Create(AzureServiceBusConnectionString.Value);

            Assert.IsInstanceOf<INamespaceManager>(manager);
        }
    }
}