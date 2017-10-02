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

            var deleteQueues = queryQueues.Result.Select(queueDescription => namespaceManager.DeleteQueueAsync(queueDescription.Path));
            var deleteTopics = queryTopics.Result.Select(topicDescription => namespaceManager.DeleteTopicAsync(topicDescription.Path));

            await TryWithRetries(Task.WhenAll(deleteQueues.Concat(deleteTopics)), 5)
                .ConfigureAwait(false);
        }

        static async Task TryWithRetries(Task task, int maxRetryAttempts, int usedRetryAttempts = 0)
        {
            try
            {
                await task
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
                when (usedRetryAttempts < maxRetryAttempts && (exception is TimeoutException || exception is MessagingCommunicationException || exception is ServerBusyException))
            {
                logger.InfoFormat("Attempt to delete all queues and topics has failed. Trying attempt {0}/{1} in 5 seconds.", usedRetryAttempts + 2, maxRetryAttempts);
                await Task.Delay(5000)
                    .ConfigureAwait(false);
                await TryWithRetries(task, maxRetryAttempts, usedRetryAttempts + 1)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.InfoFormat($"Failed to delete all queues and topics after {0}. Last received exception:\n{1}", usedRetryAttempts, exception.Message);
            }
        }

        static ILog logger = LogManager.GetLogger(typeof(Cleaner));
    }
}