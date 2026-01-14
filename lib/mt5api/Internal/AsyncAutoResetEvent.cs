using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class AsyncBroadcastEvent
    {
        // ConcurrentBag is thread-safe for adds/removals without external locking
        private readonly ConcurrentBag<TaskCompletionSource<bool>> Waiters = new ConcurrentBag<TaskCompletionSource<bool>>();

        /// <summary>
        /// Asynchronously waits for a Pulse.
        /// All current waiters will be completed when Pulse() is called.
        /// Future waiters (after a Pulse) will wait for the next Pulse.
        /// </summary>
        public Task WaitAsync()
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            Waiters.Add(tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Releases ALL currently waiting tasks immediately.
        /// Does not affect future WaitAsync calls — they will wait for the next Pulse.
        /// </summary>
        public void Pulse()
        {
            // TryTake repeatedly until the bag is empty (or appears empty)
            // This is safe because ConcurrentBag allows concurrent adds while we're draining
            while (Waiters.TryTake(out var tcs))
                tcs.TrySetResult(true);
        }
    }
}


//public async Task<bool> WaitAsync(TimeSpan timeout)
//{
//    using (var cts = new CancellationTokenSource(timeout))
//    {
//        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
//        Waiters.Add(tcs);
//        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, cts.Token));
//        return completedTask == tcs.Task;
//    }
//}