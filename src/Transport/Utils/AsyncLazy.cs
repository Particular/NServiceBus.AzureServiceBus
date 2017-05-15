namespace NServiceBus.AzureServiceBus.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Run(taskFactory))
        {
        }

        public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
    }
}