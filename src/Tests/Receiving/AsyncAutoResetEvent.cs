namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class AsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource<object>> _completionSources = new Queue<TaskCompletionSource<object>>();
        private bool m_signaled;

        public AsyncAutoResetEvent(bool signaled)
        {
            m_signaled = signaled;
        }

        public Task WaitOne()
        {
            lock (_completionSources)
            {
                var tcs = new TaskCompletionSource<object>();
                if (_completionSources.Count > 0 || !m_signaled)
                {
                    _completionSources.Enqueue(tcs);
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
            lock (_completionSources)
            {
                if (_completionSources.Count > 0) toSet = _completionSources.Dequeue();
                else m_signaled = true;
            }
            if (toSet != null) toSet.SetResult(null);
        }
    }
}