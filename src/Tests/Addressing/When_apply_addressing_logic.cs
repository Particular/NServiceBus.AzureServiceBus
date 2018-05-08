namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing
{
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_apply_addressing_logic
    {
        SanitizationStrategy sanitizationStrategy;
        CompositionStrategy compositionStrategy;
        AddressingLogic addressingLogic;

        [SetUp]
        public void SetUp()
        {
            sanitizationStrategy = new SanitizationStrategy();
            compositionStrategy = new CompositionStrategy();
            addressingLogic = new AddressingLogic(sanitizationStrategy, compositionStrategy);
        }

        [Test]
        [TestCase("validendpoint@namespaceName")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey")]
        [TestCase("endpoint$name@namespaceName")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey")]
        public void Composition_and_sanitization_should_be_invoked_for_values_with_suffix(string endpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.IsTrue(compositionStrategy.WasInvoked);
            Assert.IsTrue(sanitizationStrategy.WasInvoked);
        }

        class SanitizationStrategy : ISanitizationStrategy
        {
            public bool WasInvoked { get; private set; }

            public string Sanitize(string entityPathOrName, EntityType entityType)
            {
                WasInvoked = true;
                return entityPathOrName;
            }
        }

        class CompositionStrategy : ICompositionStrategy
        {
            public bool WasInvoked { get; private set; }

            public string GetEntityPath(string entityName, EntityType entityType)
            {
                WasInvoked = true;
                return entityName;
            }
        }
    }
}