namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Individualization
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_discriminator_individualization_strategy
    {
        [Test]
        public void Discriminator_individualization_will_append_discriminator_to_endpointname()
        {
            var strategy = new DiscriminatorBasedIndividualization();
            var endpointname = "myendpoint";
            var discriminator = "-mydiscriminator";

            strategy.SetDiscriminatorGenerator(() => discriminator);

            Assert.AreEqual(endpointname + discriminator, strategy.Individualize(endpointname));
        }
    }
}