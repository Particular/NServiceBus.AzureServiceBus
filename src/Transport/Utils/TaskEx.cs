namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;

    static class TaskEx
    {
        //TODO: remove when we update to 4.6 and can use Task.CompletedTask
        public static readonly Task Completed = Task.FromResult(0);

        public static readonly Task<bool> CompletedTrue = Task.FromResult(true);

        public static void Ignore(this Task task)
        {
        }

        public static async Task IgnoreCancellation(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}