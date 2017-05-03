namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_using_ThrowOnFailedValidation_sanitization
    {
        const string validEntityPathForQueueOrTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityPathForQueueOrTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";
        const string illegalCharacterEntityPathForQueueOrTopic = "rw3pSH5zk5aQahkzt$E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";

        const string validEntityNameForSubscriptionOrQueue = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongEntityNameForSubscriptionOrRule = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";
        const string illegalCharacterEntityNameForSubscriptionOrRule = "6pwTRR34FFr/6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";

        [TestCase(validEntityPathForQueueOrTopic, EntityType.Queue)]
        [TestCase(validEntityPathForQueueOrTopic, EntityType.Topic)]
        [TestCase(validEntityNameForSubscriptionOrQueue, EntityType.Subscription)]
        [TestCase(validEntityNameForSubscriptionOrQueue, EntityType.Topic)]
        public void Should_not_change_valid_paths_or_names(string entityPathOrName, EntityType entityType)
        {
            var settings = new SettingsHolder();
            DefaultConfigurationValues.Apply(settings);
            var sanitization = new ThrowOnFailedValidation(settings);

            var sanitizedResult = sanitization.Sanitize(entityPathOrName, entityType);

            Assert.That(sanitizedResult, Is.EqualTo(entityPathOrName));
        }

        [TestCase(illegalCharacterEntityPathForQueueOrTopic, EntityType.Queue)]
        [TestCase(illegalCharacterEntityPathForQueueOrTopic, EntityType.Topic)]
        [TestCase(illegalCharacterEntityNameForSubscriptionOrRule, EntityType.Subscription)]
        [TestCase(illegalCharacterEntityNameForSubscriptionOrRule, EntityType.Rule)]
        public void Should_throw_on_invalid_characters(string entityPathOrName, EntityType entityType)
        {
            var settings = new SettingsHolder();
            DefaultConfigurationValues.Apply(settings);
            var sanitization = new ThrowOnFailedValidation(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName, entityType));
        }

        [TestCase(tooLongEntityPathForQueueOrTopic, EntityType.Queue)]
        [TestCase(tooLongEntityPathForQueueOrTopic, EntityType.Topic)]
        [TestCase(tooLongEntityNameForSubscriptionOrRule, EntityType.Subscription)]
        [TestCase(tooLongEntityNameForSubscriptionOrRule, EntityType.Rule)]
        public void Should_throw_on_invalid_length(string entityPathOrName, EntityType entityType)
        {
            var settings = new SettingsHolder();
            DefaultConfigurationValues.Apply(settings);
            var sanitization = new ThrowOnFailedValidation(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName, entityType));
        }

    }
}