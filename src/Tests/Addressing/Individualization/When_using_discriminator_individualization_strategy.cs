namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Individualization
{
    using System;
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
#pragma warning disable 618
            var config = new AzureServiceBusIndividualizationSettings(settingsHolder);
#pragma warning restore 618
            config.UseStrategy<DiscriminatorBasedIndividualization>().DiscriminatorGenerator(endpointName => discriminator);

            var strategy = new DiscriminatorBasedIndividualization(settingsHolder);
            
            Assert.That(strategy.Individualize(endpointname), Is.EqualTo(endpointname + discriminator));
        }

        [Test]
        public void Discriminator_individualization_will_blow_up_if_no_discriminator_generator_is_registered()
        {
            const string endpointname = "myendpoint";

            var settingsHolder = new SettingsHolder();
#pragma warning disable 618
            var config = new AzureServiceBusIndividualizationSettings(settingsHolder);
#pragma warning restore 618
            config.UseStrategy<DiscriminatorBasedIndividualization>();

            var strategy = new DiscriminatorBasedIndividualization(settingsHolder);
            
            Assert.Throws<Exception>(() => strategy.Individualize(endpointname));
        }
    }
}