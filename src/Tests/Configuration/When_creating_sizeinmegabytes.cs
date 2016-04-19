namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_sizeinmegabytes
    {
        [Test]
        public void Should_be_able_to_create_custom_size()
        {
            var result = SizeInMegabytes.Create(10240);
            Assert.That((long)result, Is.EqualTo(10240));
        }
    }
}