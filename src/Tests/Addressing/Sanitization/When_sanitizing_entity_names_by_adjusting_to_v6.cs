namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_by_adjusting_to_v6
    {
        [Test]
        public void Valid_entity_names_are_returned_as_is()
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization(validation);
            var endpointname = "validendpoint";

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        [TestCase("endpoint$name", "endpointname")]
        [TestCase("endpoint/name", "endpointname")]
        public void Invalid_entity_names_will_be_stripped_from_illegal_characters_for_version_6_of_transport(string endpointName, string sanitizedName)
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization(validation);
            var sanitizedResult = sanitization.Sanitize(endpointName, EntityType.Queue);
            Assert.AreEqual(sanitizedName, sanitizedResult);
        }

        [Test]
        public void Too_long_entity_names_will_be_turned_into_a_deterministic_guid()
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization(validation);
            var endpointname = "endpointrw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
            var sanitized = MD5DeterministicNameBuilder.Build(endpointname);

            Assert.AreEqual(sanitized, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        class ValidationMock : IValidationStrategy
        {
            public ValidationResult IsValid(string entityPath, EntityType entityType)
            {
                var validationResult = new ValidationResult();
                if (entityPath.Contains("$"))
                {
                    validationResult.AddError("contains `$` sign");
                }

                if (entityPath.Length > 260)
                {
                    validationResult.AddError("lenth is more than 260 characters");
                }

                return validationResult;
            }
        }
    }
}