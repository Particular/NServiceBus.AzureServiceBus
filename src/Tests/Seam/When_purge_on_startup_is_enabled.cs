namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using Transport;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_purge_on_startup_is_enabled
    {
        [Test]
        public void Should_throw()
        {
            var container = new TransportPartsContainer();
            container.Register<TopologyOperator>();
            var settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", "sales");
            new DefaultConfigurationValues().Apply(settings);
            container.Register<ReadOnlySettings>(() => settings);

            var pump = new MessagePump(null, container, settings);
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            const bool purgeOnStartup = true;

            Assert.ThrowsAsync< InvalidOperationException> (async () => await pump.Init(context => TaskEx.Completed, null, criticalError, new PushSettings("sales", "error", purgeOnStartup, TransportTransactionMode.SendsAtomicWithReceive)));
        }
    }
}