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

            var config = new AzureServiceBusIndividualizationSettings(settingsHolder);

            var strategy = new DiscriminatorBasedIndividualization();
            config.UseStrategy(strategy).DiscriminatorGenerator(endpointName => discriminator);

            strategy.Initialize(settingsHolder);

            Assert.That(strategy.Individualize(endpointname), Is.EqualTo(endpointname + discriminator));
        }

        [Test]
        public void Discriminator_individualization_will_blow_up_if_no_discriminator_generator_is_registered()
        {
            var settingsHolder = new SettingsHolder();
            var config = new AzureServiceBusIndividualizationSettings(settingsHolder);
            var strategy = new DiscriminatorBasedIndividualization();
            config.UseStrategy(strategy);

            Assert.Throws<Exception>(() => strategy.Initialize(settingsHolder));
        }
    }
}