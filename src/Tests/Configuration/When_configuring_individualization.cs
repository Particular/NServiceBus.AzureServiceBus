namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_individualization
    {
        [Test]
        public void Should_be_able_to_set_the_sanitization_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);

            var topicSettings = extensions.Addressing().Individualization().UseStrategy<MyIndividualizationStrategy>();

            Assert.AreEqual(typeof(MyIndividualizationStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy));
        }

        class MyIndividualizationStrategy : IIndividualizationStrategy
        {
            public string Individualize(string endpointname)
            {
                throw new NotImplementedException();//not relevant to the test
            }
        }
    }
}