namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_managing_factory_lifecycle
    {
        [Test]
        public void Creates_new_factories_for_namespace()
        {
            //var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            //var lifecycleManager = new MessagingFactoryLifeCycleManager( , settings);
            throw new Exception();
        }

        [Test]
        public void Caches_factories_for_reuse()
        {
            throw new Exception();
        }

        [Test]
        public void Replaces_factories_when_closed()
        {
            throw new Exception();
        }

    }
}