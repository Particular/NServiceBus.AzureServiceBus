namespace NServiceBus.AzureServiceBus.Utils
{
    using System;
    using System.Threading.Tasks;

    class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Run(taskFactory))
        {
        }
    }
}