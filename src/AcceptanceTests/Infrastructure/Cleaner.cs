namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NUnit.Framework;
    using Utils;

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

            const int maxRetryAttempts = 5;
            var deleteQueues = (await queryQueues).Select(queueDescription => TryWithRetries(queueDescription.Path, namespaceManager.DeleteQueueAsync(queueDescription.Path), maxRetryAttempts));
            var deleteTopics = (await queryTopics).Select(topicDescription => TryWithRetries(topicDescription.Path, namespaceManager.DeleteTopicAsync(topicDescription.Path), maxRetryAttempts));

            await Task.WhenAll(deleteQueues)
                .ConfigureAwait(false);
            
            await Task.WhenAll(deleteTopics)
                .ConfigureAwait(false);
        }

        static async Task TryWithRetries(string entityPath, Task task, int maxRetryAttempts, int usedRetryAttempts = 0)
        {
            try
            {
                await task
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
                when (usedRetryAttempts < maxRetryAttempts && (exception is TimeoutException || exception.IsTransientException()))
            {
                TestContext.WriteLine($"Attempt to delete '{entityPath}' has failed. Retrying attempt {usedRetryAttempts + 1}/{maxRetryAttempts} in 5 seconds.");
                await Task.Delay(5000)
                    .ConfigureAwait(false);
                await TryWithRetries(entityPath, task, maxRetryAttempts, usedRetryAttempts + 1)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                TestContext.WriteLine($"Failed to delete '{entityPath}' after {usedRetryAttempts}. Last received exception:\n{exception.Message}");
            }
        }
    }
}