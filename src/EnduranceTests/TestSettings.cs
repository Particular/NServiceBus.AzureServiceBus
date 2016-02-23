namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    internal static class TestSettings
    {
        public static int Rate { get; private set; } = 1000;

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
