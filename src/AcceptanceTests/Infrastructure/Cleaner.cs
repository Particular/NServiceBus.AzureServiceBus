namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NUnit.Framework;

    public class Cleaner : NServiceBusAcceptanceTest
    {
        [Test, Explicit("Intended to be executed explicitly to delete all queues and topics.")]
        [Category("Cleanup")]
        public async Task DeleteEntities()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(TestUtility.GetDefaultConnectionString());

            var queryQueues = namespaceManager.GetQueuesAsync();
            var queryTopics = namespaceManager.GetTopicsAsync();
            await Task.WhenAll(queryQueues, queryTopics)
                .ConfigureAwait(false);

            var deleteQueues = queryQueues.Result.Select(queueDescription => namespaceManager.DeleteQueueAsync(queueDescription.Path));
            var deleteTopics = queryTopics.Result.Select(topicDescription => namespaceManager.DeleteTopicAsync(topicDescription.Path));

            await Task.WhenAll(deleteQueues.Concat(deleteTopics))
                .ConfigureAwait(false);
        }
    }
}