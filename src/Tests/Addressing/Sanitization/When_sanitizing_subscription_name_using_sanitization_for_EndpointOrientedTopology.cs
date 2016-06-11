namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_subscription_name_using_sanitization_for_EndpointOrientedTopology
    {
        const string validNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";

        [Test]
        public void Should_only_sanitize_subscriptions_entities()
        {
            var sanitization = new EndpointOrientedTopologySanitizationForSubscriptions(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Subscription));
        }

        [TestCase(validNameForSubscription, EntityType.Subscription)]
        public void Should_not_sanitize_valid_path(string entityPath, EntityType entityType)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForSubscriptions(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(entityPath));
        }

        [TestCase("endpoint$name", "endpointname")]
        [TestCase("endpoint/name", "endpointname")]
        public void Should_sanitize_invalid_characters(string entityPathOrName, string expectedPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForSubscriptions(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }

        [TestCase(tooLongNameForSubscription)]
        public void Should_sanitize_path_that_is_longer_the_maximum(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForSubscriptions(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            var expectedPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }
    }
}