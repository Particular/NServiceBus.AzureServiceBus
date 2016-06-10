namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_using_EndpointOrientedSanitization
    {
        const string validPathForQueueOrTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb28dAAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";

        const string validNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongNameForSubscription = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";

        [TestCase(validPathForQueueOrTopic, EntityType.Queue)]
        [TestCase(validPathForQueueOrTopic, EntityType.Topic)]
        [TestCase(validNameForSubscription, EntityType.Subscription)]
        public void Should_not_change_valid_paths_or_names(string entityPathOrName, EntityType entityType)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitization(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName, entityType);

            Assert.That(sanitizedResult, Is.EqualTo(entityPathOrName));
        }

        [TestCase("endpoint$name", EntityType.Queue, "endpointname")]
        [TestCase("endpoint/name", EntityType.Queue, "endpointname")]
        [TestCase("endpoint$name", EntityType.Topic, "endpointname")]
        [TestCase("endpoint/name", EntityType.Topic, "endpointname")]
        [TestCase("endpoint$name", EntityType.Subscription, "endpointname")]
        [TestCase("endpoint/name", EntityType.Subscription, "endpointname")]
        public void Should_sanitize_invalid_characters(string entityPathOrName, EntityType entityType, string expectedPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitization(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName, entityType);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }

        [TestCase(tooLongEntityName, EntityType.Queue)]
        [TestCase(tooLongEntityName, EntityType.Topic)]
        [TestCase(tooLongNameForSubscription, EntityType.Subscription)]
        public void Should_sanitize_longer_than_maximum_path_or_name(string entityPathOrName, EntityType entityType)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitization(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName, entityType);

            var expectedPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }
    }
}