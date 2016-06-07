namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Individualization
{
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_discriminator_individualization_strategy
    {
        [Test]
        public void Discriminator_individualization_will_append_discriminator_to_endpointname()
        {
            const string endpointname = "myendpoint";
            const string discriminator = "-mydiscriminator";

            var settingsHolder = new SettingsHolder();
            var config = new AzureServiceBusIndividualizationSettings(settingsHolder);
            config.UseStrategy<DiscriminatorBasedIndividualization>().DiscriminatorGenerator(endpointName => discriminator);

            var strategy = new DiscriminatorBasedIndividualization(settingsHolder);
            
            Assert.That(strategy.Individualize(endpointname), Is.EqualTo(endpointname + discriminator));
        }
    }
}