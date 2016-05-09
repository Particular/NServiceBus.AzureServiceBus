namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Azure.Transports.WindowsAzureServiceBus;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    class CompleteMessagesBackgroundProcess
    {
        static ILog logger = LogManager.GetLogger<CompleteMessagesBackgroundProcess>();

        readonly ConcurrentDictionary<Guid, Task> _pendingTasks;

        public CompleteMessagesBackgroundProcess()
        {
            _pendingTasks = new ConcurrentDictionary<Guid, Task>();
        }

        public void ScheduleMessageToComplete(BrokeredMessage message)
        {
            var task = new Task(() => message.SafeCompleteAsync().ConfigureAwait(false));
            task.ContinueWith(x =>
            {
                Task outTask;
                _pendingTasks.TryRemove(message.LockToken, out outTask);
            });
            _pendingTasks.TryAdd(message.LockToken, task);

            task.RunSynchronously();
        }

        public async Task Stop()
        {
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var tasks = _pendingTasks.Values;
            var finishedTask = await Task.WhenAny(Task.WhenAll(tasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                logger.Error("Process to complete messages failed to stop with in the time allowed (30s)");
            }
        }
    }
}