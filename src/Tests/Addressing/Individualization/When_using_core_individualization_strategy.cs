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

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_discriminator_individualization_strategy
    {
        [Test]
        public void Discriminator_individualization_will_append_discriminator_to_endpointname()
        {
            var strategy = new DiscriminatorBasedIndividualizationStrategy();
            var endpointname = "myendpoint";
            var discriminator = "-mydiscriminator";

            strategy.SetDiscriminatorGenerator(() => discriminator);

            Assert.AreEqual(endpointname + discriminator, strategy.Individualize(endpointname));
        }
    }
}