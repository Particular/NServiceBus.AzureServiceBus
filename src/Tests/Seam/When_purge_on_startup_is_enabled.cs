namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using AzureServiceBus;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_purge_on_startup_is_enabled
    {
        [Test]
        public void Should_throw()
        {
            var pump = new MessagePump(null, null);
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            const bool purgeOnStartup = true;

            Assert.ThrowsAsync< InvalidOperationException> (async () => await pump.Init(context => TaskEx.Completed, criticalError, new PushSettings("sales", "error", purgeOnStartup, TransportTransactionMode.SendsAtomicWithReceive)));
        }
    }
}