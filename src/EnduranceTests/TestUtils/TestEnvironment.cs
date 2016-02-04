namespace NServiceBus.AzureServiceBus.EnduranceTests.TestUtils
{
    using System;
    using Microsoft.WindowsAzure.Storage;

    internal static class TestEnvironment
    {
        public static string AzureServiceBus
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

        public static CloudStorageAccount AzureStorage
        {
            get
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.EnduranceTests.StorageConnectionString");
                if (connectionString != null)
                {
                    return CloudStorageAccount.Parse(connectionString);
                }

                throw new InvalidOperationException("Failed to get a value from AzureServiceBus.EnduranceTests.StorageConnectionString`. Please add it to your environment variables to run tests.");
            }
        }
        public static string SlackWebhookUri
        {
            get
            {
                var webUri = Environment.GetEnvironmentVariable("AzureServiceBus.EnduranceTests.SlackWebhookUri");
                return webUri;
            }
        }
    }
}
