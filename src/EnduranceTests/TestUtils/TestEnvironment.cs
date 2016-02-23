namespace NServiceBus.AzureServiceBus.EnduranceTests.TestUtils
{
    using System;
    using Microsoft.WindowsAzure.Storage;

    internal static class TestEnvironment
    {
        public static string AzureServiceBus => GetCriticalEnvironmentVariable("AzureServiceBus.ConnectionString");

        public static CloudStorageAccount AzureStorage
        {
            get
            {
                var connectionString = GetCriticalEnvironmentVariable("AzureServiceBus.EnduranceTests.StorageConnectionString");
                return CloudStorageAccount.Parse(connectionString);
            }
        }
        public static string SlackWebhookUri => GetCriticalEnvironmentVariable("AzureServiceBus.EnduranceTests.SlackWebhookUri");

        private static string GetCriticalEnvironmentVariable(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Failed to get a value from {key}. Please add it to your environment variables to run tests.");

            return value;
        }
    }
}
