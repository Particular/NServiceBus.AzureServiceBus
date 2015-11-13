namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_retrying_on_throttle
    {
        [Test]
        public async Task Can_send_brokered_messages()
        {
            var sender = new FakeMessageSender();

            var tasks = new List<Task>();

            tasks.Add(sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(10), 5));

            await Task.WhenAll(tasks);

            //validate
            Assert.IsTrue(sender.IsInvoked);
            Assert.AreEqual(1, sender.InvocationCount);
        }

        [Test]
        public void Non_server_busy_exceptions_will_bubble_up_without_retry()
        {
            var sender = new FakeMessageSender();
            sender.ExceptionToThrow = i => { throw new NotSupportedException(); };
          
            //validate
            Assert.Throws<NotSupportedException>(async () => await sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(10), 5));
            Assert.IsTrue(sender.IsInvoked);
            Assert.AreEqual(1, sender.InvocationCount);
        }

        [Test]
        public void Server_busy_exceptions_will_bubble_up_with_retry()
        {
            var sender = new FakeMessageSender();
            sender.ExceptionToThrow = i => { throw new ServerBusyException("Sorry, don't feel like serving you right now"); };

            Assert.Throws<ServerBusyException>(async () => await sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(1), 5));

            Assert.IsTrue(sender.IsInvoked);
            Assert.AreEqual(6, sender.InvocationCount); // 6 = initial invocation + 5 retries
        }

        [Test]
        public async Task Server_busy_exceptions_will_not_bubble_up_after_successfull_retry()
        {
            var sender = new FakeMessageSender();
            sender.ExceptionToThrow = i => { if (i < 4) { throw new ServerBusyException("Sorry, don't feel like serving you right now"); } };

            await sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(1), 5);

            Assert.IsTrue(sender.IsInvoked);
            Assert.AreEqual(4, sender.InvocationCount); // 4 = initial invocation + 3 retries
        }

        [Test]
        public void Server_busy_exceptions_will_delay_retry()
        {
            var sender = new FakeMessageSender();
            sender.ExceptionToThrow = i => { throw new ServerBusyException("Sorry, don't feel like serving you right now"); };

            var sw = new Stopwatch();
            sw.Start();

            Assert.Throws<ServerBusyException>(async () => await sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(5), 1));

            sw.Stop();

            Assert.IsTrue(sender.IsInvoked);
            Assert.AreEqual(2, sender.InvocationCount); // 2 = initial invocation + 1 retries
            Assert.IsTrue(sw.ElapsedMilliseconds > TimeSpan.FromSeconds(4).TotalMilliseconds);
        }

        class FakeMessageSender : IMessageSender
        {
            public bool IsClosed { get; } = false;

            public RetryPolicy RetryPolicy { get; set; }

            public bool IsInvoked { get; private set; }

            public int InvocationCount = 0;

            public Action<int> ExceptionToThrow { get; set; }

            public async Task SendAsync(BrokeredMessage message)
            {
                IsInvoked = true;
                InvocationCount++;

                ExceptionToThrow?.Invoke(InvocationCount);
            }

            public async Task SendBatchAsync(IEnumerable<BrokeredMessage> messages)
            {
                IsInvoked = true;
                InvocationCount++;

                ExceptionToThrow?.Invoke(InvocationCount);
            }
        }
    }
}