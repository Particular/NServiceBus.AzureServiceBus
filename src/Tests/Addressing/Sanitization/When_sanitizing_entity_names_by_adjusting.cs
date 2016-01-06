namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_by_adjusting
    {
        [TestCase("validendpoint", EntityType.Queue)]
        [TestCase("valid/endpoint", EntityType.Queue)]
        [TestCase("valid123", EntityType.Queue)]
        [TestCase("valid-1", EntityType.Queue)]
        [TestCase("valid_1", EntityType.Queue)]
        [TestCase("valid.1", EntityType.Queue)]
        [TestCase("validendpoint", EntityType.Topic)]
        [TestCase("valid/endpoint", EntityType.Topic)]
        [TestCase("valid123", EntityType.Topic)]
        [TestCase("valid-1", EntityType.Topic)]
        [TestCase("valid_1", EntityType.Topic)]
        [TestCase("valid.1", EntityType.Topic)]
        [TestCase("validendpoint", EntityType.Subscription)]
        [TestCase("valid123", EntityType.Subscription)]
        [TestCase("valid-1", EntityType.Subscription)]
        [TestCase("valid_1", EntityType.Subscription)]
        [TestCase("valid.1", EntityType.Subscription)]

        public void Valid_entity_names_are_returned_as_is(string endpointName, EntityType entityType)
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var sanitizedName = sanitization.Sanitize(endpointName, entityType);
            Assert.AreEqual(endpointName, sanitizedName);
        }

        [TestCase("endpoint$name", "endpointname", EntityType.Queue)]
        [TestCase("/endpoint/name", "endpoint/name", EntityType.Queue)]
        [TestCase("endpoint/name/", "endpoint/name", EntityType.Queue)]
        [TestCase("/endpoint/name/", "endpoint/name", EntityType.Queue)]
        [TestCase("endpoint$name", "endpointname", EntityType.Topic)]
        [TestCase("/endpoint/name", "endpoint/name", EntityType.Topic)]
        [TestCase("endpoint/name/", "endpoint/name", EntityType.Topic)]
        [TestCase("/endpoint/name/", "endpoint/name", EntityType.Topic)]
        [TestCase("endpoint$name", "endpointname", EntityType.Subscription)]
        [TestCase("/endpoint/name", "endpointname", EntityType.Subscription)]
        [TestCase("endpoint/name/", "endpointname", EntityType.Subscription)]
        [TestCase("/endpoint/name/", "endpointname", EntityType.Subscription)]
        public void Invalid_entity_names_will_be_stripped_from_illegal_characters(string endpointName, string expectedEndpointName, EntityType entityType)
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var sanitizedResult = sanitization.Sanitize(endpointName, entityType);

            Assert.AreEqual(expectedEndpointName, sanitizedResult);
        }

        [Test]
        public void Too_long_entity_names_will_be_turned_into_a_deterministic_guid()
        {
            var validation = new ValidationMock();
            var sanitization = new AdjustmentSanitizationStrategy(validation);
            var endpointname = "endpointrw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb/8/AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
            var sanitized = new SHA1DeterministicNameBuilder().Build(endpointname);

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