namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class AsyncAutoResetEvent
    {
        Queue<TaskCompletionSource<object>> completionSources = new Queue<TaskCompletionSource<object>>();
        bool signaled;

        public AsyncAutoResetEvent(bool signaled)
        {
            this.signaled = signaled;
        }

        public Task WaitOne()
        {
            lock (completionSources)
            {
                var tcs = new TaskCompletionSource<object>();
                if (completionSources.Count > 0 || !signaled)
                {
                    completionSources.Enqueue(tcs);
                }
                else
                {
                    tcs.SetResult(null);
                    signaled = false;
                }
                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<object> toSet = null;
            lock (completionSources)
            {
                if (completionSources.Count > 0)
                {
                    toSet = completionSources.Dequeue();
                }
                else
                {
                    signaled = true;
                }
            }
            if (toSet != null)
            {
                toSet.SetResult(null);
            }
        }
    }
}