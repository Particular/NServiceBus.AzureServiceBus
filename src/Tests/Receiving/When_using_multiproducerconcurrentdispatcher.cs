namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    public class When_using_multiproducerconcurrentdispatcher
    {
        [Test]
        public async Task Pushing_for_slots_before_start_works_when_batch_size_is_reached()
        {
            var receivedItems = new ConcurrentQueue<List<int>>[4]
            {
                new ConcurrentQueue<List<int>>(), 
                new ConcurrentQueue<List<int>>(), 
                new ConcurrentQueue<List<int>>(), 
                new ConcurrentQueue<List<int>>(), 
            };

            var countDownEvent = new CountdownEvent(16);

            // choose insanely high push interval
            var dispatcher = new MultiProducerConcurrentDispatcher<int>(batchSize: 100, pushInterval: TimeSpan.FromDays(1), maxConcurrency: 4, numberOfSlots: 4);

            var numberOfItems = await PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(dispatcher);

            dispatcher.Start((items, slot, state, token) =>
            {
                receivedItems[slot].Enqueue(new List<int>(items)); // take a copy
                if (!countDownEvent.IsSet)
                {
                    countDownEvent.Signal();
                }
                return Task.FromResult(0);
            });

            await Task.Run(() => countDownEvent.Wait(TimeSpan.FromSeconds(5)));

            var snapShotOfElementsBeforeComplete = Flatten(receivedItems).Count();

            await dispatcher.Complete();

            Assert.AreEqual(1600, snapShotOfElementsBeforeComplete);
            Assert.AreEqual(TriangularNumber(numberOfItems), Flatten(receivedItems).Sum(i => i));
        }

        [Test]
        public async Task Pushing_for_slots_after_start_works_when_batch_size_is_reached()
        {
            var receivedItems = new ConcurrentQueue<List<int>>[4]
            {
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
            };

            var countDownEvent = new CountdownEvent(16);

            // choose insanely high push interval
            var dispatcher = new MultiProducerConcurrentDispatcher<int>(batchSize: 100, pushInterval: TimeSpan.FromDays(1), maxConcurrency: 4, numberOfSlots: 4);

            dispatcher.Start((items, slot, state, token) =>
            {
                receivedItems[slot].Enqueue(new List<int>(items)); // take a copy
                if (!countDownEvent.IsSet)
                {
                    countDownEvent.Signal();
                }
                return Task.FromResult(0);
            });

            var numberOfItems = await PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(dispatcher);

            // we wait for 16 counts and then complete midway
            await Task.Run(() => countDownEvent.Wait(TimeSpan.FromSeconds(5)));

            await dispatcher.Complete();

            Assert.AreEqual(TriangularNumber(numberOfItems), Flatten(receivedItems).Sum(i => i));
        }

        [Test]
        public async Task Pushing_for_slots_before_start_works_when_pushInterval_reached()
        {
            var receivedItems = new ConcurrentQueue<List<int>>[4]
            {
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
            };

            var countDownEvent = new CountdownEvent(4);

            // choose insanely high batchSize
            var dispatcher = new MultiProducerConcurrentDispatcher<int>(batchSize: 10000, pushInterval: TimeSpan.FromMilliseconds(1), maxConcurrency: 4, numberOfSlots: 4);

            var numberOfItems = await PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(dispatcher);

            dispatcher.Start((items, slot, state, token) =>
            {
                receivedItems[slot].Enqueue(new List<int>(items)); // take a copy
                if (!countDownEvent.IsSet)
                {
                    countDownEvent.Signal();
                }
                return Task.FromResult(0);
            });

            await Task.Run(() => countDownEvent.Wait(TimeSpan.FromSeconds(5)));

            var sumOfAllElementsSeenSoFar = Flatten(receivedItems).Sum(i => i);

            await dispatcher.Complete();

            Assert.AreEqual(TriangularNumber(numberOfItems), sumOfAllElementsSeenSoFar);
            Assert.AreEqual(TriangularNumber(numberOfItems), Flatten(receivedItems).Sum(i => i), "Dispatcher complete brought more items than expected");
        }

        [Test]
        public async Task Pushing_for_slots_after_start_works_when_pushInterval_reached()
        {
            var receivedItems = new ConcurrentQueue<List<int>>[4]
            {
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
            };

            var countDownEvent = new CountdownEvent(4);

            // choose insanely high batchSize to force push interval picking up all the content
            var dispatcher = new MultiProducerConcurrentDispatcher<int>(batchSize: 10000, pushInterval: TimeSpan.FromMilliseconds(1), maxConcurrency: 4, numberOfSlots: 4);

            dispatcher.Start((items, slot, state, token) =>
            {
                receivedItems[slot].Enqueue(new List<int>(items)); // take a copy
                if (!countDownEvent.IsSet)
                {
                    countDownEvent.Signal();
                }
                return Task.FromResult(0);
            });

            var numberOfItems = await PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(dispatcher);

            await Task.Run(() => countDownEvent.Wait(TimeSpan.FromSeconds(5)));

            var sumOfAllElementsSeenSoFar = Flatten(receivedItems).Sum(i => i);

            await dispatcher.Complete();

            Assert.AreEqual(TriangularNumber(numberOfItems), sumOfAllElementsSeenSoFar);
            Assert.AreEqual(TriangularNumber(numberOfItems), Flatten(receivedItems).Sum(i => i), "Dispatcher complete brought more items than expected");
        }

        [Test]
        public async Task Pushing_and_complete_with_no_drain_without_start_empties_slots()
        {
            var pushedItems = new ConcurrentQueue<List<int>>[4]
            {
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
                new ConcurrentQueue<List<int>>(),
            };

            // choose insanely high batchSize to force push interval picking up all the content
            var dispatcher = new MultiProducerConcurrentDispatcher<int>(batchSize: 100, pushInterval: TimeSpan.FromMilliseconds(1), maxConcurrency: 4, numberOfSlots: 4);

            await PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(dispatcher);

            await dispatcher.Complete(drain: false);

            var countDownEvent = new CountdownEvent(1);
            dispatcher.Start((items, slot, state, token) =>
            {
                pushedItems[slot].Enqueue(new List<int>(items)); // take a copy
                countDownEvent.Signal();
                return Task.FromResult(0);
            });

            dispatcher.Push(1, slotNumber: 1);

            await Task.Run(() => countDownEvent.Wait(TimeSpan.FromSeconds(5)));

            await dispatcher.Complete();

            Assert.AreEqual(1, Flatten(pushedItems).Sum(i => i));
        }

        static async Task<int> PushConcurrentlyTwoThousandItemsInPackagesOfFiveHundredIntoFourSlots(MultiProducerConcurrentDispatcher<int> dispatcher)
        {
            var t1 = Task.Run(() => Parallel.For(1, 500, i =>
            {
                dispatcher.Push(slotNumber: 0, item: i);
            }));

            var t2 = Task.Run(() => Parallel.For(500, 1000, i =>
            {
                dispatcher.Push(slotNumber: 1, item: i);
            }));

            var t3 = Task.Run(() => Parallel.For(1000, 1500, i =>
            {
                dispatcher.Push(slotNumber: 2, item: i);
            }));

            await Task.WhenAll(t1, t2, t3);

            var numberOfItems = 2000;
            for (var i = 1500; i < numberOfItems + 1; i++)
            {
                dispatcher.Push(slotNumber: 3, item: i);
            }
            return numberOfItems;
        }

        int TriangularNumber(int numberOfItems)
        {
            return (numberOfItems*(numberOfItems + 1))/2;
        }

        IEnumerable<int> Flatten(ConcurrentQueue<List<int>>[] captured)
        {
            var allCaptured = new List<int>();
            foreach (var queue in captured)
            {
                foreach (var list in queue.ToArray())
                {
                    allCaptured.AddRange(list);
                }
            }
            return allCaptured.OrderBy(i => i);
        }
    }
}