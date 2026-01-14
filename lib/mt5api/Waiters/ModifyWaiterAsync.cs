using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Async waiter for order modify operations.
    /// Completes once REQUEST_EXECUTED is received, or throws on bad status or timeout.
    /// </summary>
    public sealed class ModifyWaiterAsync : IDisposable
    {
        private readonly MT5API _client;
        private readonly int _requestId;

        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<bool> _tcs;
        private volatile bool _disposed;

        private readonly ConcurrentBag<OrderProgress> _progressList = new ConcurrentBag<OrderProgress>();

        public ModifyWaiterAsync(MT5API client, int requestId, int timeoutMs)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _requestId = requestId;

            _cts = new CancellationTokenSource(timeoutMs);
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _client.ProgressWaiters.TryAdd(OnProgress, 0);

            // Automatic timeout handling
            _cts.Token.Register(() =>
            {
                if (!_tcs.Task.IsCompleted)
                    _tcs.TrySetException(new TradeTimeoutException("Trade timeout"));
            });
        }

        private void OnProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId != _requestId)
                return;

            _progressList.Add(progress);

            var st = progress.TradeResult.Status;

            // Error case
            if (st != Msg.REQUEST_ACCEPTED &&
                st != Msg.REQUEST_ON_WAY &&
                st != Msg.REQUEST_EXECUTED &&
                st != Msg.DONE &&
                st != Msg.ORDER_PLACED)
            {
                _tcs.TrySetException(new ServerException(st));
                return;
            }

            // Success case — same condition as your original code
            if (st == Msg.REQUEST_EXECUTED)
            {
                _tcs.TrySetResult(true);
            }
        }

        public async Task WaitAsync()
        {
            try
            {
                await _tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _client.ProgressWaiters.TryRemove(OnProgress, out var _);
            _cts.Dispose();
        }
    }
}
