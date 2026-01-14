using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace mtapi.mt5
{
    /// <summary>
    /// Provides safe wrappers around MT5API order operations with automatic reconnection and retry logic.
    /// </summary>
    public class OrderClientSafe
    {
        /// <summary>
        /// Maximum time in milliseconds to wait for trade execution before throwing a <see cref="TradeTimeoutException"/>.
        /// </summary>
        public int TradeTimeoutSafe = 60000;

        /// <summary>
        /// Instance of the MT5 API used to perform trading operations.
        /// </summary>
        public MT5API Api { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderClientSafe"/> class using the specified MT5API.
        /// </summary>
        /// <param name="api">An initialized and connected MT5API instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="api"/> is null.</exception>
        public OrderClientSafe(MT5API api)
        {
            Api = api ?? throw new ArgumentNullException(nameof(api));
            api.ExecutionTimeout = 15000;
        }

        private int GetId() => Api.GetRequestId();

        /// <summary>
        /// Sends a new order with automatic reconnection and retry logic.
        /// </summary>
        /// <returns>The opened order.</returns>
        /// <exception cref="TradeTimeoutException">If the order cannot be sent within the timeout period.</exception>
        public Order OrderSend(string symbol, double lots, double price, OrderType type, double sl = 0, double tp = 0, ulong deviation = 0, string comment = null,
            FillPolicy fillPolicy = FillPolicy.Any, TradeType tradeType = TradeType.Transfer, double stoplimit = 0,
            Expiration expiration = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            int id = GetId();
            DateTime start = DateTime.Now;
            var expertId = ConvertTo.LongMs(DateTime.Now);

            while ((DateTime.Now - start).TotalMilliseconds < TradeTimeoutSafe)
            {
                try
                {
                    return Api.OrderSend(symbol, lots, price, type, sl, tp, deviation, comment, expertId, fillPolicy, tradeType, stoplimit, expiration, closeByTicket, placedType);
                }
                catch (ConnectException)
                {
                    Api.Disconnect();
                    Api.Connect();
                    var match = Api.Orders.Opened.Values.FirstOrDefault(o => o.ExpertId == expertId);
                    if (match != null)
                        return match;
                }
                catch (TradeTimeoutException)
                {
                    Api.Disconnect();
                    Api.Connect();
                    var match = Api.Orders.Opened.Values.FirstOrDefault(o => o.ExpertId == expertId);
                    if (match != null)
                        return match;
                }
            }

            throw new TradeTimeoutException($"Cannot send order in {TradeTimeoutSafe / 1000} seconds");
        }

        /// <summary>
        /// Closes an existing order with retry and reconnect fallback. If closure is successful but response is missed, it falls back to order history.
        /// </summary>
        /// <returns>The closed order.</returns>
        /// <exception cref="TradeTimeoutException">If the close request fails or times out.</exception>
        public Order OrderClose(long ticket, string symbol, double price, double lots, OrderType type, ulong deviation = 0,
            FillPolicy fillPolicy = FillPolicy.Any, long closeByTicket = 0, PlacedType placedType = default)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < TradeTimeoutSafe)
            {
                try
                {
                    return Api.OrderClose(ticket, symbol, price, lots, type, deviation, fillPolicy, 0, null, closeByTicket, placedType);
                }
                catch (ConnectException)
                {
                    Api.Disconnect();
                    Api.Connect();

                    try
                    {
                        var history = Api.DownloadOrderHistory(Api.ServerTime.AddMinutes(-5), Api.ServerTime);
                        var closed = history.Orders.FirstOrDefault(o => o.Ticket == ticket && o.DealInternalOut?.Lots == lots);
                        if (closed != null)
                            return closed;
                    }
                    catch { }
                }
                catch (TradeTimeoutException)
                {
                    Api.Disconnect();
                    Api.Connect();

                    try
                    {
                        var history = Api.DownloadOrderHistory(Api.ServerTime.AddMinutes(-5), Api.ServerTime);
                        var closed = history.Orders.FirstOrDefault(o => o.Ticket == ticket);
                        if (closed != null)
                            return closed;
                    }
                    catch { }
                }
            }

            throw new TradeTimeoutException($"Cannot close order in {TradeTimeoutSafe / 1000} seconds");
        }

        /// <summary>
        /// Deletes a pending order with retries and fallback check using the open orders cache.
        /// </summary>
        /// <param name="ticket">Order ticket number.</param>
        /// <exception cref="TradeTimeoutException">If deletion fails within the timeout period.</exception>
        public void OrderDelete(long ticket, OrderType type, string symbol, double lots, double price)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < TradeTimeoutSafe)
            {
                try
                {
                    Api.OrderClose(ticket, symbol, price, lots, type);
                    return;
                }
                catch (ConnectException)
                {
                    Api.Disconnect();
                    Api.Connect();
                    if (!Api.Orders.Opened.ContainsKey(ticket))
                        return;
                }
                catch (TradeTimeoutException)
                {
                    Api.Disconnect();
                    Api.Connect();
                    if (!Api.Orders.Opened.ContainsKey(ticket))
                        return;
                }
            }

            throw new TradeTimeoutException($"Cannot delete order in {TradeTimeoutSafe / 1000} seconds");
        }

        /// <summary>
        /// Modifies an existing order with retry and reconnect logic.
        /// </summary>
        /// <param name="ticket">The ticket number of the order to modify.</param>
        /// <exception cref="TradeTimeoutException">If the modify request fails or times out.</exception>
        public void OrderModify(long ticket, string symbol, double lots, double price, OrderType type, double sl, double tp,
            long expertId = 0, double stoplimit = 0, Expiration expiration = null, string comment = null)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < TradeTimeoutSafe)
            {
                try
                {
                    Api.OrderModify(ticket, symbol, lots, price, type, sl, tp, expertId, stoplimit, expiration, comment);
                    return;
                }
                catch (ConnectException)
                {
                    Api.Connect();
                }
                catch (TradeTimeoutException)
                {
                    Api.Connect();
                }
            }

            throw new TradeTimeoutException($"Cannot modify order in {TradeTimeoutSafe / 1000} seconds");
        }
    }
}