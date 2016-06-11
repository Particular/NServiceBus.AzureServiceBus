namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_queue_path_using_sanitization_for_EndpointOrientedTopology
    {
        const string validPathForQueue = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongPathForQueue = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb28dAAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";

        [Test]
        public void Should_only_sanitize_queue_entities()
        {
            var sanitization = new EndpointOrientedTopologySanitizationForQueues(null);

            Assert.That(sanitization.CanSanitize, Is.EqualTo(EntityType.Queue));
        }

        [TestCase(validPathForQueue)]
        public void Should_not_sanitize_valid_path(string entityPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForQueues(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(entityPath));
        }

        [TestCase("endpoint$name", "endpointname")]
        [TestCase("endpoint/name", "endpointname")]
        public void Should_sanitize_invalid_characters(string entityPath, string expectedPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForQueues(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPath));
        }

        [TestCase(tooLongPathForQueue)]
        public void Should_sanitize_path_that_is_longer_the_maximum(string entityPath)
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var sanitization = new EndpointOrientedTopologySanitizationForQueues(settings);
            var sanitizedResult = sanitization.Sanitize(entityPath);

            var expectedPathOrName = MD5DeterministicNameBuilder.Build(entityPath);

            Assert.That(sanitizedResult, Is.EqualTo(expectedPathOrName));
        }
    }
}