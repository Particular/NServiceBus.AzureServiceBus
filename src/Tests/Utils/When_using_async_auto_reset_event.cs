namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Utils
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using NUnit.Framework;

    /// <summary>
    /// Inspired by https://github.com/StephenCleary/AsyncEx
    /// </summary>
    [TestFixture]
    public class When_using_async_auto_reset_event
    {
        [Test]
        public async Task WaitAsync_Unset_IsNotCompleted()
        {
            var are = new AsyncAutoResetEvent(false);

            var task = are.WaitAsync();

            var finishedTask = await Task.WhenAny(task, Task.Delay(500));

            Assert.AreNotEqual(task, finishedTask);
        }

        [Test]
        public void WaitAsync_Preset_CompletesSynchronously()
        {
            var are = new AsyncAutoResetEvent(false);

            are.Set();
            var task = are.WaitAsync();

            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void WaitAsync_AfterSet_CompletesSynchronously()
        {
            var are = new AsyncAutoResetEvent(false);

            are.Set();
            var task = are.WaitAsync();

            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void WaitAsync_Set_CompletesSynchronously()
        {
            var are = new AsyncAutoResetEvent(true);

            var task = are.WaitAsync();

            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public async Task MultipleWaitAsync_AfterSet_OnlyOneIsCompleted()
        {
            var are = new AsyncAutoResetEvent(false);

            are.Set();
            var task1 = are.WaitAsync();
            var task2 = are.WaitAsync();

            var finishedTask = await Task.WhenAny(task2, Task.Delay(500));

            Assert.IsTrue(task1.IsCompleted);
            Assert.AreNotEqual(task2, finishedTask);
        }

        [Test]
        public async Task MultipleWaitAsync_Set_OnlyOneIsCompleted()
        {
            var are = new AsyncAutoResetEvent(true);

            var task1 = are.WaitAsync();
            var task2 = are.WaitAsync();

            var finishedTask = await Task.WhenAny(task2, Task.Delay(500));

            Assert.IsTrue(task1.IsCompleted);
            Assert.AreNotEqual(task2, finishedTask);
        }

        [Test]
        public async Task MultipleWaitAsync_AfterMultipleSet_OnlyOneIsCompleted()
        {
            var are = new AsyncAutoResetEvent(false);

            are.Set();
            are.Set();
            var task1 = are.WaitAsync();
            var task2 = are.WaitAsync();

            var finishedTask = await Task.WhenAny(task2, Task.Delay(500));

            Assert.IsTrue(task1.IsCompleted);
            Assert.AreNotEqual(task2, finishedTask);
        }

        [Test]
        public void WaitAsync_PreCancelled_Set_SynchronouslyCompletesWait()
        {
            var are = new AsyncAutoResetEvent(true);
            var token = new CancellationToken(true);

            var task = are.WaitAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public async Task WaitAsync_Cancelled_DoesNotAutoReset()
        {
            var are = new AsyncAutoResetEvent(false);
            var cts = new CancellationTokenSource();

            cts.Cancel();
            var task1 = are.WaitAsync(cts.Token);
            await task1.IgnoreCancellation();

            are.Set();
            var task2 = are.WaitAsync();

            await task2;
        }

        [Test]
        public void WaitAsync_PreCancelled_Unset_SynchronouslyCancels()
        {
            var are = new AsyncAutoResetEvent(false);
            var token = new CancellationToken(true);

            var task = are.WaitAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void WaitAsync_Cancelled_ThrowsException()
        {
            var are = new AsyncAutoResetEvent(false);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task = are.WaitAsync(cts.Token);
            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }
    }
}