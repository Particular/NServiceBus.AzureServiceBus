namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_apply_addressing_logic
    {
        ValidationStrategy validationStrategy;
        SanitizationStrategy sanitizationStrategy;
        CompositionStrategy compositionStrategy;
        AddressingLogic addressingLogic;

        [SetUp]
        public void SetUp()
        {
            validationStrategy = new ValidationStrategy();
            sanitizationStrategy = new SanitizationStrategy();
            compositionStrategy = new CompositionStrategy();
            addressingLogic = new AddressingLogic(validationStrategy, sanitizationStrategy, compositionStrategy);
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpoint$name")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpoint$name")]
        public void Sanitization_strategy_should_receive_value_without_suffix(string endpointName, string expectedEndpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.AreEqual(expectedEndpointName, sanitizationStrategy.ProvidedEntityPathOrName);
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpoint$name")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpoint$name")]
        public void Validation_strategy_should_receive_value_without_suffix(string endpointName, string expectedEndpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.AreEqual(expectedEndpointName, validationStrategy.ProvidedEntityPath);
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpoint$name")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpoint$name")]
        public void Composition_strategy_should_receive_value_without_suffix(string endpointName, string expectedEndpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.AreEqual(expectedEndpointName, compositionStrategy.ProvidedEntityName);
        }

        class ValidationStrategy : IValidationStrategy
        {
            public string ProvidedEntityPath { get; private set; }

            public ValidationResult IsValid(string entityPath, EntityType entityType)
            {
                ProvidedEntityPath = entityPath;

                var validationResult = new ValidationResult();
                validationResult.AddError("fake error only for testing purpose");
                return validationResult;
            }
        }

        class SanitizationStrategy : ISanitizationStrategy
        {
            public string ProvidedEntityPathOrName { get; private set; }

            public string Sanitize(string entityPathOrName, EntityType entityType)
            {
                ProvidedEntityPathOrName = entityPathOrName;
                return entityPathOrName;
            }
        }

        class CompositionStrategy : ICompositionStrategy
        {
            public string ProvidedEntityName { get; private set; }

            public string GetEntityPath(string entityname, EntityType entityType)
            {
                ProvidedEntityName = entityname;

                return entityname;
            }
        }
    }
}