namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_queue_path_with_ThrowOnFailingSanitization_strategy
    {
        const string validEntityPathForQueue = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityPathForQueue = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";
        const string illegalCharacterEntityPathForQueue = "rw3pSH5zk5aQahkzt$E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";

        [Test]
        public void Should_only_sanitize_queue_entities()
        {
            var sanitization = new ThrowOnFailingSanitizationForQueues(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Queue));
        }

        [TestCase(validEntityPathForQueue)]
        public void Should_not_change_valid_paths_or_names(string entityPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForQueues(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(entityPath));
        }

        [TestCase(illegalCharacterEntityPathForQueue)]
        public void Should_throw_on_invalid_characters(string entityPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForQueues(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPath));
        }

        [TestCase(tooLongEntityPathForQueue)]
        public void Should_throw_on_invalid_length(string entityPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new ThrowOnFailingSanitizationForQueues(settings);

            Assert.Throws<Exception>(() => sanitization.Sanitize(entityPath));
        }
    }
}