namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Threading.Tasks;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_getting_items_from_circular_buffer
    {
        [Test]
        public void Should_be_thread_safe()
        {
            var maxDegreeOfParallelism = 5;
            var numberOfEntries = 2;

            var buffer = new CircularBuffer<BufferEntry>(numberOfEntries);

            Parallel.For(0, numberOfEntries, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item =>
            {
                buffer.Put(new BufferEntry());
            });

            Parallel.For(0, 100000, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item =>
            {
                buffer.Get();
            });

        }

        class BufferEntry
        {
        }

    }
}