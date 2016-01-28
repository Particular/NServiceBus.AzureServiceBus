namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    using System.Threading;

    public static class TestSettings
    {
        private static SemaphoreSlim enabledMutex = new SemaphoreSlim(1);
        private static SemaphoreSlim rateMutex = new SemaphoreSlim(2);

        private static bool enabled;
        private static int rate = 4000;

        public static bool TestEnabled
        {
            get { return enabled; }
            set
            {
                enabledMutex.Wait();
                enabled = value;
            }
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
