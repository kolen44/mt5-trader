using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Async waiter for market close operations.
    /// Waits until deal with PositionTicket appears in updates.
    /// </summary>
    public sealed class MarketCloseWaiterAsync : IDisposable
    {
        private readonly MT5API _client;
        private readonly int _requestId;
        private readonly long _ticket;
        private readonly double _closePrice;

        private readonly ConcurrentBag<OrderProgress> _progress = new ConcurrentBag<OrderProgress>();
        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<Order> _tcs;

        private volatile Order _resultOrder;
        private bool _disposed;

        public MarketCloseWaiterAsync(
            MT5API client,
            int requestId,
            int timeoutMs,
            long ticket,
            bool notPartialClose,
            double closePrice)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _requestId = requestId;
            _ticket = ticket;
            _closePrice = closePrice;

            _cts = new CancellationTokenSource(timeoutMs);
            _tcs = new TaskCompletionSource<Order>(TaskCreationOptions.RunContinuationsAsynchronously);

            _client.ProgressWaiters.TryAdd(OnOrderProgress, 0);
            _client.UpdateWaiters.TryAdd(OnOrderUpdate, 0);

            // Timeout
            _cts.Token.Register(() =>
            {
                if (!_tcs.Task.IsCompleted)
                    _tcs.TrySetException(new TradeTimeoutException("Trade timeout"));
            });
        }

        private void OnOrderProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId != _requestId)
                return;

            _progress.Add(progress);

            var st = progress.TradeResult.Status;

            if (st != Msg.REQUEST_ACCEPTED &&
                st != Msg.REQUEST_ON_WAY &&
                st != Msg.REQUEST_EXECUTED &&
                st != Msg.DONE &&
                st != Msg.ORDER_PLACED)
            {
                _tcs.TrySetException(new ServerException(st));
            }
        }

        private void OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.Deal == null)
                return;

            if (update.Deal.PositionTicket == _ticket)
            {
                _resultOrder = new Order(
                    new DealInternal[] { update.Deal, update.OppositeDeal },
                    _requestId,
                    _client
                );

                _tcs.TrySetResult(_resultOrder);
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

            _client.ProgressWaiters.TryRemove(OnOrderProgress, out var _);
            _client.UpdateWaiters.TryRemove(OnOrderUpdate, out var _);

            _cts?.Dispose();
        }
    }
}
