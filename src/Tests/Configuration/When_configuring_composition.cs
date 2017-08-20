namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_composition
    {
        [Test]
        public void Should_be_able_to_set_the_composition_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var strategy = new MyCompositionStrategy();

            var topicSettings = extensions.Composition().UseStrategy(strategy);

            Assert.AreSame(strategy, topicSettings.GetSettings().Get<ICompositionStrategy>(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy));
        }

        class MyCompositionStrategy : ICompositionStrategy
        {
            public void Initialize(ReadOnlySettings settings)
            {
                throw new NotImplementedException();
            }

            public string GetEntityPath(string entityName, EntityType entityType)
            {
                throw new NotImplementedException(); // not relevant to the test
            }
        }
    }
}