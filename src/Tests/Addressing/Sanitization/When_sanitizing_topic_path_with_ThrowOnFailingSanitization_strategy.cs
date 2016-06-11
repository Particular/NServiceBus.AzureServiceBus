namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_topic_path_with_ThrowOnFailingSanitization_strategy
    {
        const string validEntityPathForTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityPathForTopic = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";
        const string illegalCharacterEntityPathForTopic = "rw3pSH5zk5aQahkzt$E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";

        [Test]
        public void Should_only_sanitize_topic_entities()
        {
            var sanitization = new ThrowOnFailingSanitizationForTopics(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Topic));
        }

        [TestCase(validEntityPathForTopic)]
        public void Should_not_change_valid_paths_or_names(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForTopics(settings);
            var sanitizedResult = sanitization.Sanitize(entityPathOrName);

            Assert.That(sanitizedResult, Is.EqualTo(entityPathOrName));
        }

        [TestCase(illegalCharacterEntityPathForTopic)]
        public void Should_throw_on_invalid_characters(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForTopics(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName));
        }

        [TestCase(tooLongEntityPathForTopic)]
        public void Should_throw_on_invalid_length(string entityPathOrName)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForTopics(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPathOrName));
        }
    }
}