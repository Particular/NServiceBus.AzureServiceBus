namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Sanitization
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sanitizing_entity_names_by_throwing_exception
    {
        [Test]
        public void Valid_entity_names_are_returned_as_is()
        {
            var sanitization = new ThrowOnFailingSanitization();
            var endpointname = "validendpoint";
            var validationResult = new ValidationMock(shouldValidationPass: true).IsValid(endpointname, EntityType.Queue);

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue, validationResult));
        }

        [Test]
        public void Invalid_entity_names_result_in_exception()
        {
            var sanitization = new ThrowOnFailingSanitization();
            var endpointname = "invalidendpoint";
            var validationResult = new ValidationMock(shouldValidationPass:false).IsValid(endpointname, EntityType.Queue);

            Assert.Throws<Exception>(() => sanitization.Sanitize(endpointname, EntityType.Queue, validationResult));
        }

        class ValidationMock : IValidationStrategy
        {
            bool shouldValidationPass;

            public ValidationMock(bool shouldValidationPass)
            {
                this.shouldValidationPass = shouldValidationPass;
            }

            public ValidationResult IsValid(string entityPath, EntityType entityType)
            {
                var validationResult = new ValidationResult();

                if (!shouldValidationPass)
                {
                    validationResult.AddErrorForInvalidLength("validation should fail");
                }

                return validationResult;
            }
        }
    }
}