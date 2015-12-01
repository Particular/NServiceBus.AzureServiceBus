namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_core_individualization_strategy
    {
        [Test]
        public void Core_individualization_will_just_return_endpointname_as_provided_by_core()
        {
            var strategy = new CoreIndividualizationStrategy();
            var endpointname = "myendpoint";

            Assert.AreEqual(endpointname, strategy.Individualize(endpointname));
        }
    }
}