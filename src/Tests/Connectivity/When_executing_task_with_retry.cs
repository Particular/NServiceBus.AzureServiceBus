namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Connectivity
{
    using System;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using FakeItEasy;
    using Microsoft.ServiceBus.Messaging;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]

    public class When_executing_task_with_retry
    {
        [Test]
        public void Should_not_retry_if_number_of_retries_is_not_specified()
        {
            var messageSender = A.Fake<IMessageSender>();
            var message = new BrokeredMessage();
            var numberOfRetries = 0;
            var totalAttempts = 0;

            A.CallTo(() => messageSender.Send(message))
                .Invokes(() => totalAttempts++)
                .Throws(new ServerBusyException("busy, come later"));

            Assert.That(async () => await messageSender.RetryOnThrottleAsync(s => s.Send(message), s => s.Send(message), TimeSpan.Zero, numberOfRetries), Throws.TypeOf<ServerBusyException>());
            Assert.AreEqual(1, totalAttempts);
        }

        [Test]
        public void Should_retry_as_number_of_specified_retries()
        {
            var messageSender = A.Fake<IMessageSender>();
            var message = new BrokeredMessage();
            var numberOfRetries = 3;
            var totalAttempts = 0;

            A.CallTo(() => messageSender.Send(message))
                .Invokes(() => totalAttempts++)
                .Throws(new ServerBusyException("busy, come later"));

            Assert.That(async () => await messageSender.RetryOnThrottleAsync(s => s.Send(message), s => s.Send(message), TimeSpan.Zero, numberOfRetries), Throws.TypeOf<ServerBusyException>());
            Assert.AreEqual(1 + numberOfRetries, totalAttempts);
        }

        [Test]
        public Task Should_not_throw_for_successful_retry()
        {
            var messageSender = A.Fake<IMessageSender>();
            var brokeredMessage = new BrokeredMessage();

            A.CallTo(() => messageSender.Send(brokeredMessage))
                .ReturnsNextFromSequence(Task.Run(() => { throw new ServerBusyException("busy, come later"); }), TaskEx.Completed);

            return messageSender.RetryOnThrottleAsync(s => s.Send(brokeredMessage), s => s.Send(brokeredMessage), TimeSpan.Zero, 1);
        }
    }
}