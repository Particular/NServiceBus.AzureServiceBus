namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
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

            const int maxRetryAttempts = 5;
            var deleteQueues = (await queryQueues).Select(queueDescription => TryWithRetries(queueDescription.Path, namespaceManager.DeleteQueueAsync(queueDescription.Path), maxRetryAttempts));
            var deleteTopics = (await queryTopics).Select(topicDescription => TryWithRetries(topicDescription.Path, namespaceManager.DeleteTopicAsync(topicDescription.Path), maxRetryAttempts));

            await Task.WhenAll(deleteQueues.Concat(deleteTopics))
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
                when (usedRetryAttempts < maxRetryAttempts && (exception is TimeoutException || exception is MessagingCommunicationException || exception is ServerBusyException))
            {
                logger.Info($"Attempt to delete '{entityPath}' has failed. Trying attempt {usedRetryAttempts + 2}/{maxRetryAttempts} in 5 seconds.");
                await Task.Delay(5000)
                    .ConfigureAwait(false);
                await TryWithRetries(entityPath, task, maxRetryAttempts, usedRetryAttempts + 1)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.Info($"Failed to delete '{entityPath}' after {usedRetryAttempts}. Last received exception:\n{exception.Message}");
            }
        }

        static ILog logger = LogManager.GetLogger(typeof(Cleaner));
    }
}