namespace NServiceBus.AzureServiceBus.EnduranceTests.TestUtils
{
    using System;

    internal static class AzureServiceBusConnectionString
    {
        public static string Value
        {
            get
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
                if (connectionString != null)
                {
                    return connectionString;
                }

                throw new InvalidOperationException("Failed to get a value from `AzureServiceBus.ConnectionString`. Please add it to your environment variables to run tests.");
            }
        }
    }
}
