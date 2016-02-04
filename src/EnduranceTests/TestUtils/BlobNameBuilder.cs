namespace NServiceBus.AzureServiceBus.EnduranceTests.TestUtils
{
    using System;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using NLog;

    public static class BlobNameBuilder
    {
        public static string Build(LogEventInfo logEvent)
        {
            return $"{logEvent.TimeStamp.Ticks}_{logEvent.Exception.GetType().Name}";
        }

        public static string BuildContainerName(LogEventInfo logEvent)
        {
            var name = logEvent.LoggerName.Replace(".", "-").Replace(" ", "").ToLower();
            return name.Length > 63 ? name.Substring(0, 63) : name;
        }

        public static string Build(CloudStorageAccount storageAccount, LogEventInfo logEvent)
        {
            var sap = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddYears(1))
            };

            var containerName = BuildContainerName(logEvent);

            return $"{storageAccount.BlobEndpoint}{containerName}/{Build(logEvent)}{storageAccount.CreateCloudBlobClient().GetContainerReference(containerName).GetSharedAccessSignature(sap)}";
        }
    }
}
