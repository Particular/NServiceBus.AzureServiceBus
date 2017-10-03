namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;

    static class TestUtility
    {
        static string connectionString;
        static string fallbackConnectionString;

        public static string DefaultConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    var environmentVariableName = $"{nameof(AzureServiceBusTransport)}.ConnectionString";
                    connectionString = EnvironmentHelper.GetEnvironmentVariable(environmentVariableName);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new Exception($"Oh no! Could not find an environment variable '{environmentVariableName}'.");
                    }
                }
                return connectionString;
            }
        }

        public static string FallbackConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(fallbackConnectionString))
                {
                    var environmentVariableName = $"{nameof(AzureServiceBusTransport)}.ConnectionString";
                    fallbackConnectionString = EnvironmentHelper.GetEnvironmentVariable(environmentVariableName);
                    if (string.IsNullOrEmpty(fallbackConnectionString))
                    {
                        throw new Exception($"Oh no! Could not find an environment variable '{environmentVariableName}'.");
                    }
                }
                return fallbackConnectionString;
            }
        }
    }
}