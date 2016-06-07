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
            var validation = new ValidationMock(shouldValidationPass:true);
            var sanitization = new ThrowOnFailingSanitization(validation);
            var endpointname = "validendpoint";

            Assert.AreEqual(endpointname, sanitization.Sanitize(endpointname, EntityType.Queue));
        }

        [Test]
        public void Invalid_entity_names_result_in_exception()
        {
            var validation = new ValidationMock(shouldValidationPass:false);
            var sanitization = new ThrowOnFailingSanitization(validation);
            var endpointname = "invalidendpoint";

            Assert.Throws<Exception>(() => sanitization.Sanitize(endpointname, EntityType.Queue));
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
                    validationResult.AddError("validation should fail");
                }

                return validationResult;
            }
        }
    }
}