namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public static class TaskWithRetryExtensions
    {
        public static Task RetryOnThrottle(this IMessageSender sender, Func<IMessageSender, Task> action, TimeSpan delay, int maxRetryAttempts, int retryAttempts=0)
        {
            var task = action(sender);

            return task.ContinueWith(async t =>
            {
                var serverBusy = task.Exception?.InnerException as ServerBusyException;

                if (serverBusy != null && retryAttempts < maxRetryAttempts)
                {
                    await Task.Delay(delay);
                    await sender.RetryOnThrottle(action, delay, maxRetryAttempts, ++retryAttempts);
                }
                else if (t.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }
            })
            .Unwrap();
        }
    }
}