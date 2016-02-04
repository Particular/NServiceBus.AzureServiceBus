
namespace NServiceBus.AzureServiceBus.EnduranceTests.Targets
{
    using System.Diagnostics;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using NLog;
    using NLog.Targets;
    using NServiceBus.AzureServiceBus.EnduranceTests.TestUtils;
    using LogLevel = NLog.LogLevel;

    public class AzureBlobErrorOnlyTarget : TargetWithLayout
    {
        private CloudBlobClient _client;

        public CloudStorageAccount StorageAccount { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            _client = StorageAccount.CreateCloudBlobClient();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Level < LogLevel.Error || logEvent.Exception == null) return;

            if (_client == null) return;

            var container = _client.GetContainerReference(BlobNameBuilder.BuildContainerName(logEvent));
            container.CreateIfNotExists();

            var blobName = BlobNameBuilder.Build(logEvent);

            var blob = container.GetBlockBlobReference(blobName);

            var index = 0;
            while (blob.Exists())
            {
                index++;
                blob = container.GetBlockBlobReference(blobName + "_" + index);
            }

            blob.Properties.ContentType = "text/plain";

            var logMessage = Layout.Render(logEvent);

            blob.UploadText(logMessage);

            Debug.WriteLine("Writing to blob storage: " + blob.Uri);
        }
    }
}
