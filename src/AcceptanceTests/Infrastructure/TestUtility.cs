namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;

    static class TestUtility
    {
        public static string GetDefaultConnectionString()
        {
            var environmentVariableName = $"{nameof(AzureServiceBusTransport)}.ConnectionString";
            var connectionString = EnvironmentHelper.GetEnvironmentVariable(environmentVariableName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Oh no! Could not find an environment variable '{environmentVariableName}'.");
            }
            return connectionString;
        }
    }
}