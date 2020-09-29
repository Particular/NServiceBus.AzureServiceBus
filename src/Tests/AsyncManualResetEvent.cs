namespace NServiceBus.AzureServiceBus
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Inspired by http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx
    /// </summary>
    class AsyncManualResetEvent
    {
        public AsyncManualResetEvent(bool initialState)
        {
            if (initialState)
            {
                tcs.SetResult(true);
            }
        }

        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(state =>
            {
                var source = (TaskCompletionSource<bool>)state;
                source.TrySetCanceled();
            }, tcs))
            {
                await tcs.Task.ConfigureAwait(false);
            }
        }

        public void Set()
        {
            tcs.TrySetResult(true);
        }

        public void Reset()
        {
            var sw = new SpinWait();

            do
            {
                var tcs1 = tcs;
                if (!tcs1.Task.IsCompleted)
                {
                    return;
                }

                var taskCompletionSource = new TaskCompletionSource<bool>();
#pragma warning disable 420
                if (Interlocked.CompareExchange(ref tcs, taskCompletionSource, tcs1) == tcs1)
#pragma warning restore 420
                {
                    return;
                }

                sw.SpinOnce();
            }
            while (true);
        }

        volatile TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
    }
}