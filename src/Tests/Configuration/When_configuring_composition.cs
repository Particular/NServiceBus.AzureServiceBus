namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
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

            var topicSettings = extensions.Topology().Addressing().Composition().Strategy<MyCompositionStrategy>();

            Assert.AreEqual(typeof(MyCompositionStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy));
        }

        class MyCompositionStrategy : ICompositionStrategy
        {
            public string GetFullPath(string entityname)
            {
                throw new NotImplementedException(); // not relevant to the test
            }
        }
    }
}