namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    using System;
    using System.Threading;

    internal static class TestSettings
    {
        private static SemaphoreSlim rateMutex = new SemaphoreSlim(1);

        private static int rate = 1000;

        public static Guid TestRunId { get; set; }

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

        public static int SlackBufferSize { get; set; } = 5;
        public static int SlackTimeoutInMilliseconds { get; set; } = 1000*60*60*8;
    }
}
