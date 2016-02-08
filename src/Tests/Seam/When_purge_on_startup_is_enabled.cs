namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_purge_on_startup_is_enabled
    {
        MessagePump pump;

        [SetUp]
        public void SetUp()
        {
            pump = new MessagePump(null, null);
        }

        [Test]
        public Task Should_throw()
        {
            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => Task.FromResult(0));

            Assert.Throws<InvalidOperationException>(async () => await pump.Init(context => TaskEx.Completed, criticalError, new PushSettings("sales", "error", true, TransportTransactionMode.SendsAtomicWithReceive)));

            return Task.FromResult(true);
        }
    }
}