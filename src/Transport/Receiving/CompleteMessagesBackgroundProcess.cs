namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Azure.Transports.WindowsAzureServiceBus;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    class CompleteMessagesBackgroundProcess : ICompleteMessagesScheduler
    {
        static ILog logger = LogManager.GetLogger<CompleteMessagesBackgroundProcess>();

        readonly ConcurrentDictionary<Task, Task> _pendingTasks;

        public CompleteMessagesBackgroundProcess()
        {
            _pendingTasks = new ConcurrentDictionary<Task, Task>();
        }

        public void ScheduleMessageToComplete(BrokeredMessage message)
        {
            var task = Task.Run(message.SafeCompleteAsync);
            _pendingTasks.TryAdd(task, task);

            task.ContinueWith(x =>
            {
                Task outTask;
                _pendingTasks.TryRemove(task, out outTask);
            });
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

    public interface ICompleteMessagesScheduler
    {
        void ScheduleMessageToComplete(BrokeredMessage message);
    }
}