namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_by_throwing
    {
        [Test]
        public void Valid_entity_names_are_returned_as_is()
        {
            var validation = new ValidationMock(true);
            var sanitization = new ThrowOnInvalidSanitizationStrategy(validation);
            var endpointname = "validendpoint";

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        [Test]
        public void Invalid_entity_names_result_in_exception()
        {
            var validation = new ValidationMock(false);
            var sanitization = new ThrowOnInvalidSanitizationStrategy(validation);
            var endpointname = "invalidendpoint";

            Assert.Throws<EndpointValidationException>(() => sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        class ValidationMock : IValidationStrategy
        {
            bool _result;

            public ValidationMock(bool result)
            {
                _result = result;
            }

            public bool IsValid(string entityPath, EntityType entityType)
            {
                return _result;
            }
        }
    }
}