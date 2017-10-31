namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.Logging;
    using NUnit.Framework;

    public class Cleaner : NServiceBusAcceptanceTest
    {
        static ILog logger = LogManager.GetLogger<Cleaner>();

        [Test, Explicit("Intended to be executed explicitly to delete all queues and topics.")]
        [Category("Cleanup")]
        public async Task DeleteEntities()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
            await DeleteEntities(namespaceManager);

            namespaceManager = NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value);
            await DeleteEntities(namespaceManager);
        }

        static async Task DeleteEntities(NamespaceManager namespaceManager)
        {
            var queryQueues = namespaceManager.GetQueuesAsync();
            var queryTopics = namespaceManager.GetTopicsAsync();
            await Task.WhenAll(queryQueues, queryTopics)
                .ConfigureAwait(false);

            const int maxRetryAttempts = 5;
            var deleteQueues = (await queryQueues).Select(queueDescription => TryWithRetries(queueDescription.Path, namespaceManager.DeleteQueueAsync(queueDescription.Path), maxRetryAttempts));

            await Task.WhenAll(deleteQueues)
                .ConfigureAwait(false);

            var deleteTopics = (await queryTopics).Select(topicDescription => TryWithRetries(topicDescription.Path, namespaceManager.DeleteTopicAsync(topicDescription.Path), maxRetryAttempts));
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
                logger.Info($"Attempt to delete '{entityPath}' has failed. Retrying attempt {usedRetryAttempts + 1}/{maxRetryAttempts} in 5 seconds.");
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
    }

    static class AzureServiceBusConnectionString
    {
        public static string Value
        {
            get
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
                if (connectionString != null)
                {
                    return connectionString;
                }

                throw new InvalidOperationException("Failed to get a value from `AzureServiceBus.ConnectionString`. Please add it to your environment variables to run tests.");
            }
        }
    }

    static class ExceptionExtensions
    {
        public static bool IsTransientException(this Exception exception)
        {
            var messagingException = exception as MessagingException;
            return messagingException?.IsTransient ?? (exception is TimeoutException || exception is OperationCanceledException);
        }
    }
}