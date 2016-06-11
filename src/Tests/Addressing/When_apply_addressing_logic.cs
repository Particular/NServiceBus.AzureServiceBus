namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_apply_addressing_logic
    {
        FakeSanitizationStrategy sanitizationStrategy;
        FakeCompositionStrategy compositionStrategy;
        AddressingLogic addressingLogic;

        [SetUp]
        public void SetUp()
        {
            compositionStrategy = new FakeCompositionStrategy();
            sanitizationStrategy = new FakeSanitizationStrategy();

            var settings = new SettingsHolder();
            var sanitizationSettings = new AzureServiceBusSanitizationSettings(settings);
            sanitizationSettings.UseStrategy(sanitizationStrategy);

            addressingLogic = new AddressingLogic(settings, compositionStrategy);
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpoint$name")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpoint$name")]
        public void Sanitization_strategy_should_receive_value_without_suffix(string endpointName, string expectedEndpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.AreEqual(expectedEndpointName, sanitizationStrategy.ProvidedEntityPath);
        }

        [Test]
        [TestCase("validendpoint@namespaceName", "validendpoint")]
        [TestCase("validendpoint@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "validendpoint")]
        [TestCase("endpoint$name@namespaceName", "endpoint$name")]
        [TestCase("endpoint$name@Endpoint=sb://namespaceName.servicebus.windows.net;SharedAccessKeyName=SharedAccessKeyName;SharedAccessKey=SharedAccessKey", "endpoint$name")]
        public void Composition_strategy_should_receive_value_without_suffix(string endpointName, string expectedEndpointName)
        {
            addressingLogic.Apply(endpointName, EntityType.Queue);

            Assert.AreEqual(expectedEndpointName, sanitizationStrategy.ProvidedEntityPath);
        }

        class FakeSanitizationStrategy : SanitizationStrategy
        {
            public string ProvidedEntityPath { get; private set; }

            public override string Sanitize(string entityPathOrName)
            {
                ProvidedEntityPath = entityPathOrName;
                return entityPathOrName;
            }

            public override EntityType CanSanitize { get; } = EntityType.Queue;
        }

        class FakeCompositionStrategy : ICompositionStrategy
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