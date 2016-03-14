namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.TestUtils
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

        public static string Fallback
        {
            get
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");
                if (connectionString != null)
                {
                    return connectionString;
                }

                throw new InvalidOperationException("Failed to get a value from `AzureServiceBus.ConnectionString.Fallback`. Please add it to your environment variables to run tests.");
            }
        }
    }
}
