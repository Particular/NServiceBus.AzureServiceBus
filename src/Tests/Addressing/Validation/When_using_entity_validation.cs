namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_entity_validation
    {
        const string validEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
        const string tooLongEntityName = "rw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLETooLong";
        const string illegalCharacterEntityName = "rw3pSH5zk5aQahkzt$E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";

        const string validSubscriptionName = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";
        const string tooLongSubscriptionName = "6pwTRR34FFr.6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vkTooLong";
        const string illegalCharacterSubscriptionName = "6pwTRR34FFr/6YhPi-iDNfdSRLNDFIqZ97_Ky64w49r50n72vk";

        [Test]
        public void Namespaces_allow_queues_with_paths_up_to_260_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_allow_queues_with_slashes_dashes_dots_and_underscores()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_do_not_allow_queues_with_paths_over_260_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(tooLongEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_do_not_allows_queues_with_illegal_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(illegalCharacterEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_allow_topics_with_paths_up_to_260_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validEntityName, EntityType.Topic));
        }

        [Test]
        public void Namespaces_allow_topics_with_slashes_dashes_dots_and_underscores()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_do_not_allow_topics_with_paths_over_260_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(tooLongEntityName, EntityType.Topic));
        }

        [Test]
        public void Namespaces_do_not_allow_topics_with_illegal_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(illegalCharacterEntityName, EntityType.Queue));
        }

        [Test]
        public void Namespaces_allow_subscriptions_with_paths_up_to_50_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validSubscriptionName, EntityType.Subscription));
        }

        [Test]
        public void Namespaces_allow_subscriptions_with_dashes_dots_and_underscores()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsTrue(validation.IsValid(validSubscriptionName, EntityType.Subscription));
        }

        [Test]
        public void Namespaces_do_not_allow_topics_with_paths_over_50_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(tooLongSubscriptionName, EntityType.Subscription));
        }

        [Test]
        public void Namespaces_do_not_allow_subscription_with_illegal_characters()
        {
            var validation = new EntityNameValidationRules();

            Assert.IsFalse(validation.IsValid(illegalCharacterSubscriptionName, EntityType.Subscription));
        }
    }
}