namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_topic_path_using_sanitization_for_EndpointOrientedTopology
    {
        const string validPathForQueueOrTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb28dAAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";

        const string validNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";

        [Test]
        public void Should_only_sanitize_topic_entities()
        {
            var sanitization = new EndpointOrientedTopologySanitizationForTopics(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Topic));
        }

        [TestCase(validPathForQueueOrTopic, EntityType.Topic)]
        public void Should_not_sanitize_valid_path(string entityPath, EntityType entityType)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForTopics(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(entityPath));
        }

        [TestCase("endpoint$name", EntityType.Topic, "endpointname")]
        [TestCase("endpoint/name", EntityType.Topic, "endpointname")]
        public void Should_sanitize_invalid_characters(string entityPathOrName, EntityType entityType, string expectedPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForTopics(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }

        [TestCase(tooLongEntityName, EntityType.Topic)]
        public void Should_sanitize_path_that_is_longer_the_maximum(string entityPathOrName, EntityType entityType)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForTopics(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            var expectedPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }
    }
}