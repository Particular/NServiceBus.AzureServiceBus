namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay("IsSet = {signaled}")]
    class AsyncAutoResetEvent
    {
        public AsyncAutoResetEvent(bool signaled)
        {
            this.signaled = signaled;
        }

        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            Task ret;
            lock (completionSources)
            {
                if (signaled)
                {
                    signaled = false;
                    ret = completedTask;
                }
                else
                {
                    var tcs = new TaskCompletionSource<object>();
                    var registration = cancellationToken.Register(state =>
                    {
                        var t = (TaskCompletionSource<object>) state;
                        t.TrySetCanceled();
                    }, tcs);
                    completionSources.Enqueue(Tuple.Create(tcs, registration));
                    ret = tcs.Task;
                }
            }

            return ret;
        }

        public void Set()
        {
            Tuple<TaskCompletionSource<object>, CancellationTokenRegistration> toSet = null;
            lock (completionSources)
            {
                if (completionSources.Count == 0)
                {
                    signaled = true;
                }
                else
                {
                    toSet = completionSources.Dequeue();
                }
            }

            if (toSet != null)
            {
                toSet.Item1.TrySetResult(null);
                toSet.Item2.Dispose();
            }
        }

        Queue<Tuple<TaskCompletionSource<object>, CancellationTokenRegistration>> completionSources = new Queue<Tuple<TaskCompletionSource<object>, CancellationTokenRegistration>>();
        bool signaled;
        static Task completedTask = Task.FromResult(0);
    }
}