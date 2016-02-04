namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public static class TestSettings
    {
        private static SemaphoreSlim enabledMutex = new SemaphoreSlim(1);
        private static SemaphoreSlim rateMutex = new SemaphoreSlim(2);

        private static int rate = 1000;
        private static ConcurrentBag<Guid> testRunIds;

        public static ConcurrentBag<Guid> TestRunIds
        {
            get { return testRunIds; }
            set { testRunIds = value; }
        }

        public static void ClearTestRuns()
        {
            var newBag = new ConcurrentBag<Guid>();
            Interlocked.Exchange(ref testRunIds, newBag);
        }

        public static int Rate
        {
            get { return rate; }
            set
            {
                rateMutex.Wait();
                rate = value;
            }
        }

        public static SendOptions SendOptions
        {
            get
            {
                var options = new SendOptions();
                options.RouteToLocalEndpointInstance();
                return options;
            }
        }
    }
}
