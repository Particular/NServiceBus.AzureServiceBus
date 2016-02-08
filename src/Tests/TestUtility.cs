namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;

    internal  static class TestUtility
    {
        public static async Task Delete(params string[] queueNames)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
            foreach (var queueName in queueNames)
            {
                await namespaceManager.DeleteQueueAsync(queueName).ConfigureAwait(false);
            }
        }

    }
}