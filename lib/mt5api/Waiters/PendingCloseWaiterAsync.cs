using System;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Async waiter for pending close (cancellation) completion.
    /// Waits until the order with the specified ticket becomes Cancelled.
    /// </summary>
    public sealed class PendingCloseWaiterAsync : IDisposable
    {
        private readonly MT5API _client;
        private readonly int _requestId;
        private readonly long _ticket;

        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<Order> _tcs;

        private volatile OrderProgress _progress;
        private volatile Order _closedOrder;
        private bool _disposed;

        public PendingCloseWaiterAsync(MT5API client, int requestId, int timeoutMs, long ticket)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _requestId = requestId;
            _ticket = ticket;

            _cts = new CancellationTokenSource(timeoutMs);
            _tcs = new TaskCompletionSource<Order>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Register waiters
            _client.ProgressWaiters.TryAdd(OnProgress, 0);
            _client.UpdateWaiters.TryAdd(OnUpdate, 0);

            // Timeout
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

            _progress = progress;

            var status = progress.TradeResult.Status;

            // Error statuses
            if (status != Msg.REQUEST_ACCEPTED &&
                status != Msg.REQUEST_ON_WAY &&
                status != Msg.REQUEST_EXECUTED &&
                status != Msg.DONE &&
                status != Msg.ORDER_PLACED)
            {
                _tcs.TrySetException(new ServerException(status));
            }
        }

        private void OnUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.OrderInternal == null)
                return;

            if (update.OrderInternal.TicketNumber == _ticket &&
                update.OrderInternal.State == OrderState.Cancelled)
            {
                _closedOrder = new Order(update.OrderInternal, _client);
                _closedOrder.RequestId = _requestId;
                _tcs.TrySetResult(_closedOrder);
            }
        }

        public async Task<Order> WaitAsync()
        {
            try
            {
                return await _tcs.Task.ConfigureAwait(false);
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
            _client.UpdateWaiters.TryRemove(OnUpdate, out var _);

            _cts?.Dispose();
        }
    }
}
