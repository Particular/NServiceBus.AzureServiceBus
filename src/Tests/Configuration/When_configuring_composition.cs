namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using NServiceBus.AzureServiceBus;
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
            var extensions = new AzureServiceBusTopologySettings(settings);

            var topicSettings = extensions.Addressing().Composition().UseStrategy<MyCompositionStrategy>();

            Assert.AreEqual(typeof(MyCompositionStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy));
        }

        class MyCompositionStrategy : ICompositionStrategy
        {
            public string GetEntityPath(string entityname, EntityType entityType)
            {
                throw new NotImplementedException(); // not relevant to the test
            }
        }
    }
}