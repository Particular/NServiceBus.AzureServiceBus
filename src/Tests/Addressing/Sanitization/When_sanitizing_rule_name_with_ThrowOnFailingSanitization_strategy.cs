namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_rule_name_with_ThrowOnFailingSanitization_strategy
    {
        const string validEntityNameForRule = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongEntityNameForRule = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";
        const string illegalCharacterEntityNameForRule = "6pwTRR34FFr/6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";

        [Test]
        public void Should_only_sanitize_rule_entities()
        {
            var sanitization = new ThrowOnFailingSanitizationForRules(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Rule));
        }

        [TestCase(validEntityNameForRule)]
        public void Should_not_change_valid_paths_or_names(string entityName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForRules(settings);
            var sanitizedResult = sanitization.Sanitize(entityName);

            Assert.That(sanitizedResult, Is.EqualTo(entityName));
        }

        [TestCase(illegalCharacterEntityNameForRule)]
        public void Should_throw_on_invalid_characters(string entityName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForRules(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityName));
        }

        [TestCase(tooLongEntityNameForRule)]
        public void Should_throw_on_invalid_length(string entityName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForRules(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityName));
        }

    }
}