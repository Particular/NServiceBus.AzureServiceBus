namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Threading.Tasks;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_putting_items_into_circular_buffer
    {
        [Test]
        public void Should_be_thread_safe()
        {
            var maxDegreeOfParallelism = 5;
            var numberOfEntries = 2;
            var numberOfItems = 100000;

            var buffer = new CircularBuffer<BufferEntry>(numberOfEntries, true); // overflow must be allowed

            Parallel.For(0, numberOfItems, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item =>
            {
                buffer.Put(new BufferEntry());
            });

        }

        class BufferEntry
        {
        }

    }
}