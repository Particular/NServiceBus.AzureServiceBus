namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_messaging_factories
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