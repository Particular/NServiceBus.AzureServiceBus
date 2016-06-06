namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Validation
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_entity_validation_v6_rules
    {
        const string validEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";
        const string illegalCharacterEntityName = "rw3pSH5zk5aQahkzt$E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";

        const string validSubscriptionName = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongSubscriptionName = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";
        const string illegalCharacterSubscriptionName = "6pwTRR34FFr/6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";

        [Test]
        public void Namespaces_allow_queues_with_paths_up_to_260_characters_with_slashes_dashes_dots_and_underscores()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(validEntityName, EntityType.Queue);

            Assert.IsTrue(validationResult.IsValid);
            Assert.That(validationResult.Errors, Is.Empty);
        }

        [Test]
        public void Namespaces_do_not_allow_queues_with_paths_over_260_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(tooLongEntityName, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Namespaces_do_not_allows_queues_with_illegal_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(illegalCharacterEntityName, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [TestCase("/queue")]
        [TestCase("queue/")]
        public void Namespaces_do_not_allows_queues_with_leading_or_trailing_slash(string entityPath)
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(entityPath, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Namespaces_allow_topics_with_paths_up_to_260_characters_with_slashes_dashes_dots_and_underscores()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(validEntityName, EntityType.Topic);

            Assert.IsTrue(validationResult.IsValid);
            Assert.That(validationResult.Errors, Is.Empty);
        }


        [Test]
        public void Namespaces_do_not_allow_topics_with_paths_over_260_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(tooLongEntityName, EntityType.Topic);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Namespaces_do_not_allow_topics_with_illegal_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(illegalCharacterEntityName, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [TestCase("/topic")]
        [TestCase("topic/")]
        public void Namespaces_do_not_allow_topics_with_leading_or_trailing_slash(string entityPath)
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(entityPath, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Namespaces_allow_subscriptions_with_paths_up_to_50_characters_with_dashes_dots_and_underscores()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(validSubscriptionName, EntityType.Subscription);

            Assert.IsTrue(validationResult.IsValid);
            Assert.That(validationResult.Errors, Is.Empty);
        }

        [Test]
        public void Namespaces_do_not_allow_topics_with_paths_over_50_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(tooLongSubscriptionName, EntityType.Subscription);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Namespaces_do_not_allow_subscription_with_illegal_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(illegalCharacterSubscriptionName, EntityType.Subscription);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [TestCase("illegal/queue")]
        [TestCase("illegal/queue/")]
        [TestCase("/illegal/queue")]
        [TestCase("/illegal/queue/")]
        public void Namespaces_do_not_allow_queues_with_forward_slashes(string queuePath)
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(queuePath, EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        [TestCase("illegal/topic")]
        [TestCase("illegal/topic/")]
        [TestCase("/illegal/topic")]
        [TestCase("/illegal/topic/")]
        public void Namespaces_do_not_allow_topics_with_forward_slashes(string topicPath)
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationV6Rules(settingsHolder);
            var validationResult = validation.IsValid(topicPath, EntityType.Topic);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(1));
        }

        public void Should_return_2_error_messages_for_length_and_invalid_characters()
        {
            var settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);

            var validation = new EntityNameValidationRules(settingsHolder);
            var validationResult = validation.IsValid(tooLongEntityName + "$$$", EntityType.Queue);

            Assert.IsFalse(validationResult.IsValid);
            Assert.That(validationResult.Errors.Count, Is.EqualTo(2));
        }
    }
}