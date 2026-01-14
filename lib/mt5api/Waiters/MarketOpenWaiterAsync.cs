using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    class MarketOpenWaiterAsync
    {
        private readonly MT5API Client;
        private readonly int Id;
        private readonly TimeSpan Timeout;
        private readonly ConcurrentBag<OrderProgress> Progresses = new ConcurrentBag<OrderProgress>();
        private readonly ConcurrentBag<OrderUpdate> Updates = new ConcurrentBag<OrderUpdate>();
        private readonly TaskCompletionSource<Order> Tcs = new TaskCompletionSource<Order>(TaskCreationOptions.RunContinuationsAsynchronously);

        private long Ticket = 0;

        public MarketOpenWaiterAsync(MT5API client, int id, int timeoutMs)
        {
            Client = client;
            Id = id;
            Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            Client.ProgressWaiters.TryAdd(OnProgress, 0);
            Client.UpdateWaiters.TryAdd(OnUpdate, 0);
        }

        private void OnUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.Deal == null)
                return;

            // Before ticket known → buffer updates
            if (Ticket == 0)
            {
                Updates.Add(update);
                return;
            }

            // After ticket known → check if matches
            if (update.Deal.OrderTicket == Ticket || update.Deal.PositionTicket == Ticket)
            {
                var order = new Order(new[] { update.Deal }, Id, Client);
                order.RequestId = Id;

                if (Client.AccountMethod == AccMethod.Hedging)
                    Client.Orders.Opened.TryAdd(order.Ticket, order);

                Tcs.TrySetResult(order);
            }
        }

        private void OnProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId != Id)
                return;

            Progresses.Add(progress);

            var status = progress.TradeResult.Status;

            // Error status?
            if (status != Msg.REQUEST_ACCEPTED &&
                status != Msg.REQUEST_ON_WAY &&
                status != Msg.REQUEST_EXECUTED &&
                status != Msg.DONE &&
                status != Msg.ORDER_PLACED)
            {
                Tcs.TrySetException(new ServerException(status));
                return;
            }

            // Ticket received?
            if (progress.TradeResult.TicketNumber != 0 && Ticket == 0)
            {
                Ticket = progress.TradeResult.TicketNumber;

                // Try match buffered updates
                foreach (var update in Updates)
                {
                    if (update.Deal.OrderTicket == Ticket || update.Deal.PositionTicket == Ticket)
                    {
                        var order = new Order(new[] { update.Deal }, Id, Client);
                        order.RequestId = Id;

                        if (Client.AccountMethod == AccMethod.Hedging)
                            Client.Orders.Opened.TryAdd(order.Ticket, order);

                        Tcs.TrySetResult(order);
                        return;
                    }
                }
            }
        }

        public async Task<Order> WaitAsync()
        {
            using (var cts = new CancellationTokenSource(Timeout))
                try
                {
                    using (cts.Token.Register(() =>
                        Tcs.TrySetException(new TradeTimeoutException("Trade timeout")), useSynchronizationContext: false))
                    {
                        return await Tcs.Task.ConfigureAwait(false);
                    }
                }
                finally
                {
                    // Clean up handlers
                    Client.ProgressWaiters.TryRemove(OnProgress, out _);
                    Client.UpdateWaiters.TryRemove(OnUpdate, out _);
                }
        }
    }
}
