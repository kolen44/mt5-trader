using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Async waiter for pending order placement completion.
    /// Waits until server assigns a ticket AND the corresponding Order appears in updates.
    /// </summary>
    public sealed class PendingOpenWaiterAsync : IDisposable
    {
        private readonly MT5API _client;
        private readonly int _requestId;
        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<Order> _tcs;

        private readonly ConcurrentBag<Order> _orders = new ConcurrentBag<Order>();
        private volatile OrderProgress _progress;
        private volatile Order _matchedOrder;
        private long _ticket = 0;
        private bool _disposed;

        public PendingOpenWaiterAsync(MT5API client, int requestId, int timeoutMs = 30000)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _requestId = requestId;

            _cts = new CancellationTokenSource(timeoutMs);
            _tcs = new TaskCompletionSource<Order>(TaskCreationOptions.RunContinuationsAsynchronously);

            _client.ProgressWaiters.TryAdd(OnOrderProgress, 0);
            _client.UpdateWaiters.TryAdd(OnOrderUpdate, 0);

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

            _progress = progress;

            var status = progress.TradeResult.Status;

            if (status != Msg.REQUEST_ACCEPTED &&
                status != Msg.REQUEST_ON_WAY &&
                status != Msg.REQUEST_EXECUTED &&
                status != Msg.DONE &&
                status != Msg.ORDER_PLACED)
            {
                _tcs.TrySetException(new ServerException(status));
                return;
            }

            if (progress.TradeResult.TicketNumber != 0 &&
                progress.TradeResult.Status == Msg.REQUEST_EXECUTED)
            {
                _ticket = progress.TradeResult.TicketNumber;
                TryMatchOrder();
            }
        }

        private void OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.OrderInternal == null)
                return;

            if (_ticket == 0)
            {
                _orders.Add(new Order(update.OrderInternal, _client));
            }
            else if (update.OrderInternal.TicketNumber == _ticket)
            {
                _matchedOrder = new Order(update.OrderInternal, _client);
                TryMatchOrder();
            }
        }

        private void TryMatchOrder()
        {
            if (_ticket == 0 || _matchedOrder != null)
                return;

            foreach (var o in _orders)
            {
                if (o.Ticket == _ticket)
                {
                    _matchedOrder = o;
                    break;
                }
            }

            if (_matchedOrder != null)
            {
                _matchedOrder.RequestId = _requestId;
                _tcs.TrySetResult(_matchedOrder);
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
