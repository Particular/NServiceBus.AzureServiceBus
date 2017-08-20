namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_individualization
    {
        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var strategy = new MyIndividualizationStrategy();
            var topicSettings = extensions.Individualization().UseStrategy(strategy);

            Assert.AreSame(strategy, topicSettings.GetSettings().Get<IIndividualizationStrategy>(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy));
        }

        class MyIndividualizationStrategy : IIndividualizationStrategy
        {
            public void Initialize(ReadOnlySettings settings)
            {
                throw new NotImplementedException();
            }

            public string Individualize(string endpointName)
            {
                throw new NotImplementedException();//not relevant to the test
            }
        }
    }
}