namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_by_adjusting
    {
        [Test]
        public void Valid_entity_names_are_returned_as_is()
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var endpointname = "validendpoint";

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        [Test]
        public void Invalid_entity_names_will_be_stripped_from_illegal_characters()
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var endpointname = "endpoint$name";
            var sanitized = "endpointname";

            Assert.AreEqual(sanitized, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        [Test]
        public void Too_long_entity_names_will_be_turned_into_a_deterministic_guid()
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var endpointname = "endpointrw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
            var sanitized = new DeterministicGuidBuilder().Build("endpointrw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE").ToString();

            Assert.AreEqual(sanitized, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        class ValidationMock : IValidationStrategy
        {
            public bool IsValid(string entityPath, EntityType entityType)
            {
                return !entityPath.Contains("$") && entityPath.Length < 260;
            }
        }
    }
}