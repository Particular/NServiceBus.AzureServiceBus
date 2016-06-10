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
            var endpointname = "validendpoint";
            var validationResult = new ValidationResult();
            var sanitization = new EndpointOrientedTopologySanitization();

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue, validationResult));
        }

        [TestCase("endpoint$name", "endpointname")]
        [TestCase("endpoint/name", "endpointname")]
        public void Invalid_entity_names_will_be_stripped_from_illegal_characters_for_version_6_of_transport(string endpointName, string sanitizedName)
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization();
            var validationResult = validation.IsValid(endpointName, EntityType.Queue);
            var sanitizedResult = sanitization.Sanitize(endpointName, EntityType.Queue, validationResult);

            Assert.AreEqual(sanitizedName, sanitizedResult);
        }

        [Test]
        public void Too_long_entity_names_will_be_turned_into_a_deterministic_guid()
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization();
            var endpointname = "endpointrw3pSH5zk5aQahkzt-E_U0aPf6KbXpWMZ7vnRFb_8_AAptt5Gp6YVt3rSnWwREBx3-BgnqNw9ol-Rn.wFRTFR1UzoCuHZM443EqKvSt-fzpMHPusH8rm4OQeiBCwBRVDA29rLC6RlOBZ4Xs_h415HW2lAdOPR6j4L-CaaVkfnDO2-9bjUTAGCDKs6jWYmgoCYMBx6x5PS_e0nRT05S_J78qd3SOKWTM-YjVj9fwQZ9xG2x02uCW-XIh0siprJp9c3jLE";
            var sanitized = MD5DeterministicNameBuilder.Build(endpointname);
            var validationResult = validation.IsValid(endpointname, EntityType.Queue);

            Assert.AreEqual(sanitized, sanitization.Sanitize(endpointname, EntityType.Queue, validationResult));
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpointname")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpointname")]
        public void Only_queue_name_without_suffix_should_be_sanitized(string endpointName, string expectedEndpointName)
        {
            var validation = new ValidationMock();
            var sanitization = new EndpointOrientedTopologySanitization();
            var validationResult = validation.IsValid(endpointName, EntityType.Queue);
            var sanitizedResult = sanitization.Sanitize(endpointName, EntityType.Queue, validationResult);

            Assert.AreEqual(expectedEndpointName, sanitizedResult);
        }

        class ValidationMock : IValidationStrategy
        {
            public ValidationResult IsValid(string entityPath, EntityType entityType)
            {
                var validationResult = new ValidationResult();
                if (entityPath.Contains("$"))
                {
                    validationResult.AddErrorForInvalidCharacter("contains `$` sign");
                }

                if (entityPath.Length > 260)
                {
                    validationResult.AddErrorForInvalidLength("lenth is more than 260 characters");
                }

                return validationResult;
            }
        }
    }
}