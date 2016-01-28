namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Tests;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
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
        public async Task Should_throw()
        {
            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => Task.FromResult(0));

            Assert.Throws<ConfigurationErrorsException>(async () => await pump.Init(context => TaskEx.Completed, criticalError, new PushSettings("sales", "error", true, TransportTransactionMode.SendsAtomicWithReceive)));
        }
    }
}