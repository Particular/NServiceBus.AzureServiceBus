namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;

    static class TaskWithRetryExtensions
    {
        static ILog logger = LogManager.GetLogger(typeof(TaskWithRetryExtensions));

        public static async Task RetryOnThrottleAsync(this IMessageSenderInternal sender, Func<IMessageSenderInternal, Task> action, Func<IMessageSenderInternal, Task> retryAction, TimeSpan delay, int maxRetryAttempts, int retryAttempts = 0)
        {
            try
            {
                // upon retries have to use new BrokeredMessage instances
                var actionToTake = retryAttempts == 0 ? action : retryAction;
                await actionToTake(sender).ConfigureAwait(false);
            }
            catch (ServerBusyException)
            {
                if (retryAttempts < maxRetryAttempts)
                {
                    logger.Warn($"We are throttled, backing off for {delay.TotalSeconds} seconds (attempt {retryAttempts + 1}/{maxRetryAttempts}).");

                    await Task.Delay(delay).ConfigureAwait(false);
                    await sender.RetryOnThrottleAsync(action, retryAction, delay, maxRetryAttempts, ++retryAttempts).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}