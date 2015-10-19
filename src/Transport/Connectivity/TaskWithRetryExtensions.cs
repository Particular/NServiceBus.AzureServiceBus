namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;

    static class TaskWithRetryExtensions
    {
        static ILog logger = LogManager.GetLogger(typeof(TaskWithRetryExtensions));

        public static Task RetryOnThrottle(this IMessageSender sender, Func<IMessageSender, Task> action, TimeSpan delay, int maxRetryAttempts, int retryAttempts = 0)
        {
            var task = action(sender);

            return task.ContinueWith(async executedTask =>
            {
                var serverBusy = executedTask.Exception?.InnerException as ServerBusyException; // We might need to use ExceptionDispatchInfo

                if (serverBusy != null && retryAttempts < maxRetryAttempts)
                {
                    logger.Warn($"We are throttled, backing off for {delay.TotalSeconds} seconds (attempt {retryAttempts + 1}/{maxRetryAttempts}).");

                    await Task.Delay(delay).ConfigureAwait(false);
                    await sender.RetryOnThrottle(action, delay, maxRetryAttempts, ++retryAttempts).ConfigureAwait(false);
                }
                else if (executedTask.IsFaulted)
                {
                    throw executedTask.Exception.InnerException;
                }
            }).Unwrap();
        }
    }
}