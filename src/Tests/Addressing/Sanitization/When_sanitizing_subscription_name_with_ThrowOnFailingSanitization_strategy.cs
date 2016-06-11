namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_subscription_name_with_ThrowOnFailingSanitization_strategy
    {
        const string validEntityNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongEntityNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";
        const string illegalCharacterEntityNameForSubscription = "6pwTRR34FFr/6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";

        [Test]
        public void Should_only_sanitize_subscription_entities()
        {
            var sanitization = new ThrowOnFailingSanitizationForSubscription(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Subscription));
        }

        [TestCase(validEntityNameForSubscription)]
        public void Should_not_change_valid_paths_or_names(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForSubscription(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(entityPathOrName));
        }

        [TestCase(illegalCharacterEntityNameForSubscription)]
        public void Should_throw_on_invalid_characters(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForSubscription(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName));
        }

        [TestCase(tooLongEntityNameForSubscription)]
        public void Should_throw_on_invalid_length(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForSubscription(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName));
        }
    }
}