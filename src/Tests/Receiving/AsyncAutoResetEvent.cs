namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class AsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource<object>> completionSources = new Queue<TaskCompletionSource<object>>();
        private bool m_signaled;

        public AsyncAutoResetEvent(bool signaled)
        {
            m_signaled = signaled;
        }

        public Task WaitOne()
        {
            lock (completionSources)
            {
                var tcs = new TaskCompletionSource<object>();
                if (completionSources.Count > 0 || !m_signaled)
                {
                    completionSources.Enqueue(tcs);
                }
                else
                {
                    tcs.SetResult(null);
                    m_signaled = false;
                }
                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<object> toSet = null;
            lock (completionSources)
            {
                if (completionSources.Count > 0) toSet = completionSources.Dequeue();
                else m_signaled = true;
            }
            if (toSet != null) toSet.SetResult(null);
        }
    }
}