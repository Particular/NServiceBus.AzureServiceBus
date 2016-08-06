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

            var topicSettings = extensions.Individualization().UseStrategy<MyIndividualizationStrategy>();

            Assert.AreEqual(typeof(MyIndividualizationStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy));
        }

        class MyIndividualizationStrategy : IIndividualizationStrategy
        {
            public string Individualize(string endpointName)
            {
                throw new NotImplementedException();//not relevant to the test
            }
        }
    }
}