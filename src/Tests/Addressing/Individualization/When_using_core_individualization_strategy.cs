namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Individualization
{
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_core_individualization_strategy
    {
        [Test]
        public void Core_individualization_will_just_return_endpointname_as_provided_by_core()
        {
#pragma warning disable 618
            var strategy = new CoreIndividualization();
#pragma warning restore 618
            var endpointname = "myendpoint";

            Assert.AreEqual(endpointname, strategy.Individualize(endpointname));
        }
    }
}