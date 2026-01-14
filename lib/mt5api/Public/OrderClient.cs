//#define NOREQUOTE

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    public class Expiration
    {
        public ExpirationType Type;
        public DateTime DateTime;
    }

    public partial class MT5API
    {
        internal ConcurrentDictionary<OnOrderProgress, byte> ProgressWaiters = new ConcurrentDictionary<OnOrderProgress, byte>();
        internal ConcurrentDictionary<OnOrderUpdate, byte> UpdateWaiters = new ConcurrentDictionary<OnOrderUpdate, byte>();

        /// <summary>
        /// Maximum ms to wait for execution
        /// </summary>
        public int ExecutionTimeout = 30000;

        /// <summary>
        /// Manually or by expert
        /// </summary>
        public PlacedType PlacedType = PlacedType.Manually;

        private static int RequestId = 1;
        static readonly object RequestIdLock = new object();

        /// <summary>
        /// Account information
        /// </summary>
		public AccountRec Account { get; internal set; }

		/// <summary>
		/// Get uniq request ID for async trading
		/// </summary>
		/// <returns></returns>
		public int GetRequestId()
        {
            lock (RequestIdLock)
            {
                if (RequestId == int.MaxValue)
                    RequestId = 1;
                return RequestId++;
            }
        }

        /// <summary>
        /// Send order and don't wait execution. Use OnOrderProgress event to get result.
        /// </summary>
        /// <param name="requestId">Uniq temporary ID that can be used before ticket would be assigned. You can use GetRequestID()</param>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">Lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop Loss</param>
        /// <param name="tp">Take Profit</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="comment">String comment</param>
        /// <param name="expertID">Also known as magic number</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        /// <param name="tradeType">Allows to specify execution type</param>
        /// <param name="stoplimit">StopLimit price</param>
        public void OrderSendAsync(int requestId, string symbol, double lots, double price, OrderType type, double sl = 0, double tp = 0, ulong deviation = 0, string comment = null
            , long expertID = 0, FillPolicy fillPolicy = FillPolicy.Any, TradeType tradeType = TradeType.Transfer,
            double stoplimit = 0, Expiration expiration = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            var action = TradeAction.Buy;
            if(type == OrderType.Sell || type == OrderType.SellStop || type == OrderType.SellLimit || type == OrderType.SellStopLimit)
                action = TradeAction.Sell;
            EnsureTradingAllowed(symbol, action);
            if (placedType == default)
                placedType = PlacedType;
            TradeRequest req = new TradeRequest();
            req.ByCloseTicket = closeByTicket;
            req.Flags &= ~0x100;
            req.Flags &= ~0x200;
            req.PlacedType = placedType;
            req.Login = User;
            req.Digits = Symbols.GetInfo(symbol).Digits;
            req.OrderPrice = stoplimit;
            req.Price = Math.Round(price, 8);
            req.Volume = (ulong)(Math.Round(lots * 100000000, 0));
            req.Currency = symbol;
            req.RequestId = requestId;
            if (tradeType == TradeType.Transfer)
                if (type == OrderType.Buy || type == OrderType.Sell)
                    req.TradeType = (TradeType)((int)Symbols.GetGroup(symbol).TradeType + 1);
                else
                    req.TradeType = TradeType.SetOrder;
            else
                req.TradeType = tradeType;
            if (closeByTicket != 0)
            {
                req.OrderType = OrderType.CloseBy;
            }else 
             req.OrderType = type;
            req.StopLoss = sl;
            req.TakeProfit = tp;
            req.Deviation = deviation;
            req.Comment = comment;
            req.ExpertId = expertID;
            if(fillPolicy == FillPolicy.Any)
                req.FillPolicy = GetFP(symbol, type);
            if (expiration != null)
            {
                req.ExpirationType = expiration.Type;
                if(expiration.DateTime != new DateTime()) 
                    req.ExpirationTime = ConvertTo.Long(expiration.DateTime);
            }
            new OrderSender(Connection).Send(req);
        }



        /// <summary>
        /// Send order and don't wait execution. Use OnOrderProgress event to get result.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">Lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop Loss</param>
        /// <param name="tp">Take Profit</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="comment">String comment</param>
        /// <param name="expertID">Also known as magic number</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        /// <param name="tradeType">Allows to specify execution type</param>
        /// <param name="stoplimit">StopLimit price</param>
        public Order OrderSend(string symbol, double lots, double price, OrderType type, double sl = 0, double tp = 0, ulong deviation = 0, string comment = null
            , long expertID = 0, FillPolicy fillPolicy = FillPolicy.Any, TradeType tradeType = TradeType.Transfer, double stoplimit = 0
            , Expiration expiration = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            if (placedType == default)
                placedType = PlacedType;
#if NOREQUOTE
            while(true)
				try
				{
#endif
            Order order;
            var id = GetRequestId();
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                var waiter = new MarketOpenWaiter(this, id, ExecutionTimeout);
                OrderSendAsync(id, symbol, lots, price, type, sl, tp, deviation, comment, expertID, fillPolicy, tradeType
                    , stoplimit, expiration, closeByTicket, placedType);
                order = waiter.Wait();
                if (!Subscriber.Subscribed(symbol))
                    _ = Subscriber.Subscribe(symbol);
            }
            else
            {
                var waiter = new PendingOpenWaiter(this, id, ExecutionTimeout);
                OrderSendAsync(id, symbol, lots, price, type, sl, tp, deviation, comment, expertID, fillPolicy, tradeType
                    , stoplimit, expiration, closeByTicket, placedType);
                order = waiter.Wait();
            }
            if (AccountMethod == AccMethod.Hedging)
                if (Orders.Opened.TryGetValue(order.Ticket, out var value))
                    value.Update(order);
                else
                    Orders.Opened.TryAdd(order.Ticket, order);
            _ = UpdateProfitsTask();
            return order;
#if NOREQUOTE
                }
                catch (ServerException ex)
				{
                    if (ex.Code == Msg.REQUOTE || ex.Code == Msg.NO_PRICES)
                    {
                        while (GetQuote(symbol) == null)
                            Thread.Sleep(1);
                        if (type == OrderType.Buy)
                            price = GetQuote(symbol).Ask;
                        else if (type == OrderType.Sell)
                            price = GetQuote(symbol).Bid;
                        continue;
                    }
                }
#endif

        }


        /// <summary>
        /// Send order and don't wait execution. Use OnOrderProgress event to get result.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">Lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop Loss</param>
        /// <param name="tp">Take Profit</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="comment">String comment</param>
        /// <param name="expertID">Also known as magic number</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        /// <param name="tradeType">Allows to specify execution type</param>
        /// <param name="stoplimit">StopLimit price</param>
        public async Task<Order> OrderSendAsyncTask(string symbol, double lots, double price, OrderType type, double sl = 0, double tp = 0, ulong deviation = 0, string comment = null
            , long expertID = 0, FillPolicy fillPolicy = FillPolicy.Any, TradeType tradeType = TradeType.Transfer, double stoplimit = 0
            , Expiration expiration = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            if (placedType == default)
                placedType = PlacedType;
#if NOREQUOTE
            while(true)
				try
				{
#endif
            Order order;
            var id = GetRequestId();
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                var waiter = new MarketOpenWaiterAsync(this, id, ExecutionTimeout);
                OrderSendAsync(id, symbol, lots, price, type, sl, tp, deviation, comment, expertID, fillPolicy, tradeType
                        , stoplimit, expiration, closeByTicket, placedType);
                order = await waiter.WaitAsync();
                if (!Subscriber.Subscribed(symbol))
                    _ = Subscriber.Subscribe(symbol);
            }
            else
            {
                using (var waiter = new PendingOpenWaiterAsync(this, id, ExecutionTimeout))
                {
                    OrderSendAsync(id, symbol, lots, price, type, sl, tp, deviation, comment, expertID, fillPolicy, tradeType
                        , stoplimit, expiration, closeByTicket, placedType);
                    order = await waiter.WaitAsync();
                }
            }
            if (AccountMethod == AccMethod.Hedging)
                if (Orders.Opened.TryGetValue(order.Ticket, out var value))
                    value.Update(order);
                else
                    Orders.Opened.TryAdd(order.Ticket, order);
            _ = UpdateProfitsTask();
            return order;
#if NOREQUOTE
                }
                catch (ServerException ex)
				{
                    if (ex.Code == Msg.REQUOTE || ex.Code == Msg.NO_PRICES)
                    {
                        while (GetQuote(symbol) == null)
                            Thread.Sleep(1);
                        if (type == OrderType.Buy)
                            price = GetQuote(symbol).Ask;
                        else if (type == OrderType.Sell)
                            price = GetQuote(symbol).Bid;
                        continue;
                    }
                }
#endif

        }

        /// <summary>
        /// Send order close request and don't wait execution. Use OnOrderProgress event to get result.
        /// </summary>
        /// <param name="requestId">Uniq temporary ID that can be used before ticket would be assigned. You can use GetID()</param>
        /// <param name="ticket">Order ticket</param>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">How many lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        public void OrderCloseAsync(int requestId, long ticket, string symbol, double price, double lots, OrderType type, ulong deviation = 0,
            FillPolicy fillPolicy = FillPolicy.Any, long expertId = 0, string comment = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            EnsureTradingAllowed(symbol, TradeAction.Close);
            if (placedType == default)
                placedType = PlacedType;
            TradeRequest req = new TradeRequest();
            req.ByCloseTicket = closeByTicket;
            req.Flags &= ~0x100;
            req.Flags &= ~0x200;
            req.PlacedType = placedType;
            req.Login = User;
            req.Digits = Symbols.GetInfo(symbol).Digits;
            req.RequestId = requestId;
            req.Volume = (ulong)Math.Round(lots * 100000000, 0);
            req.ExpertId = expertId;
            req.Comment = comment;
            if (req.Volume == 0)
                throw new Exception("Request Lots = 0 ");
            req.Currency = symbol;
            if(fillPolicy == FillPolicy.Any)
                req.FillPolicy = GetFP(symbol, type);
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                req.Price = Math.Round(price, 8);
                req.DealTicket = ticket;
                req.TradeType = (TradeType)((int)Symbols.GetGroup(symbol).TradeType + 1);
                if (closeByTicket != 0)
                {
                    req.TradeType = TradeType.ClosePosition;
                    req.OrderType = OrderType.Buy;
                }
                else if (type == OrderType.Buy)
                    req.OrderType = OrderType.Sell;
                else if (type == OrderType.Sell)
                    req.OrderType = OrderType.Buy;
                req.Deviation = deviation;
                //req.Flags = 2;
                //MarketCloseRequests.Add(ticket, requestId);
            }
            else
            {
                req.OrderTicket = ticket;
                req.TradeType = TradeType.CancelOrder;
                req.OrderType = type;
                //PendingCloseRequests.Add(ticket, requestId);
            }
            new OrderSender(Connection).Send(req);
        }

        /// <summary>
        /// Send order close request and wait for execution. 
        /// </summary>
        /// <param name="ticket">Order ticket</param>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">Volume</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        /// <returns>Closed order</returns>
        public Order OrderClose(long ticket, string symbol, double price, double lots, OrderType type, ulong deviation = 0, 
            FillPolicy fillPolicy = FillPolicy.Any, long expertId = 0, string comment = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            if (placedType == default)
                placedType = PlacedType;
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                var id = GetRequestId();
                bool notPartialClose = false;
                if (Orders.Opened.TryGetValue(ticket, out var order))
                    if (Math.Round(order.Lots, 8) == Math.Round(lots, 8))
                        notPartialClose = true;
                var waiter = new MarketCloseWaiter(this, id, ExecutionTimeout, ticket, notPartialClose, price);
                OrderCloseAsync(id, ticket, symbol, price, lots, type, deviation, fillPolicy, expertId, comment, closeByTicket, placedType);
                return waiter.Wait();
            }
            else
            {
                var id = GetRequestId();
                var waiter = new PendingCloseWaiter(this, id, ExecutionTimeout, ticket);
                OrderCloseAsync(id, ticket, symbol, price, lots, type, deviation, fillPolicy, expertId, comment, closeByTicket, placedType);
                return waiter.Wait();
            }
        }

        /// <summary>
        /// Send order close request and wait for execution. 
        /// </summary>
        /// <param name="ticket">Order ticket</param>
        /// <param name="symbol">Symbol</param>
        /// <param name="lots">Volume</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="deviation">Max deviation from specified price also known as Slppage</param>
        /// <param name="fillPolicy">Fill policy depends on symbol settings on broker</param>
        /// <returns>Closed order</returns>
        public async Task<Order> OrderCloseAsyncTask(long ticket, string symbol, double price, double lots, OrderType type, ulong deviation = 0,
            FillPolicy fillPolicy = FillPolicy.Any, long expertId = 0, string comment = null, long closeByTicket = 0, PlacedType placedType = default)
        {
            if (placedType == default)
                placedType = PlacedType;
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                var id = GetRequestId();
                bool notPartialClose = false;
                if (Orders.Opened.TryGetValue(ticket, out var order))
                    if (Math.Round(order.Lots, 8) == Math.Round(lots, 8))
                        notPartialClose = true;
                using (var waiter = new MarketCloseWaiterAsync(this, id, ExecutionTimeout, ticket, notPartialClose, price))
                {
                    OrderCloseAsync(id, ticket, symbol, price, lots, type, deviation, fillPolicy, expertId, comment, closeByTicket, placedType);
                    return await waiter.WaitAsync();
                }
            }
            else
            {
                var id = GetRequestId();
                using (var waiter = new PendingCloseWaiterAsync(this, id, ExecutionTimeout, ticket))
                {
                    OrderCloseAsync(id, ticket, symbol, price, lots, type, deviation, fillPolicy, expertId, comment, closeByTicket, placedType);
                    return await waiter.WaitAsync();
                }
            }
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="ticket">Ticket mnumber</param>
        /// <param name="symbol">Symbol name</param>
        /// <param name="lots">How many lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop loss</param>
        /// <param name="tp">Take profit</param>
        /// <param name="deviation">Maximum deviation in points</param>
        /// <param name="comment">Comment</param>
        /// <param name="expertID">Expert id</param>
        /// <param name="fillPolicy">Fill policy</param>
        /// <param name="stoplimit">StopLimit price</param>
        public void OrderModify(long ticket, string symbol, double lots, double price, OrderType type, double sl, double tp, long expertID = 0, double stoplimit = 0
            , Expiration expiration = null, string comment = null)
        {
            var id = GetRequestId();
            var waiter = new ModifyWaiter(this, id, ExecutionTimeout);
            OrderModifyAsync(id, ticket, symbol, lots, price, type, sl, tp, expertID, stoplimit, expiration, comment);
            waiter.Wait();
        }

        /// <summary>
        /// Modify order
        /// </summary>
        /// <param name="ticket">Ticket mnumber</param>
        /// <param name="symbol">Symbol name</param>
        /// <param name="lots">How many lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop loss</param>
        /// <param name="tp">Take profit</param>
        /// <param name="deviation">Maximum deviation in points</param>
        /// <param name="comment">Comment</param>
        /// <param name="expertID">Expert id</param>
        /// <param name="fillPolicy">Fill policy</param>
        /// <param name="stoplimit">StopLimit price</param>
        public async Task OrderModifyAsyncTask(long ticket, string symbol, double lots, double price, OrderType type, double sl, double tp, long expertID = 0, double stoplimit = 0
            , Expiration expiration = null, string comment = null)
        {
            var id = GetRequestId();
            using (var waiter = new ModifyWaiterAsync(this, id, ExecutionTimeout))
            {
                OrderModifyAsync(id, ticket, symbol, lots, price, type, sl, tp, expertID, stoplimit, expiration, comment);
                await waiter.WaitAsync();
            }
        }
        /// <summary>
        /// OrderModifyAsync
        /// </summary>
        /// <param name="requestId">Request ID to identify order in OrderProgress before Ticket assigned</param>
        /// <param name="ticket">Ticket mnumber</param>
        /// <param name="symbol">Symbol name</param>
        /// <param name="lots">How many lots</param>
        /// <param name="price">Price</param>
        /// <param name="type">Order type</param>
        /// <param name="sl">Stop loss</param>
        /// <param name="tp">Take profit</param>
        /// <param name="deviation">Maximum deviation in points</param>
        /// <param name="comment">Comment</param>
        /// <param name="expertID">Expert id</param>
        /// <param name="fillPolicy">Fill policy</param>
        /// <param name="stoplimit">StopLimit price</param>
        public void OrderModifyAsync(int requestId, long ticket, string symbol, double lots, double price, OrderType type
            , double sl, double tp, long expertID = 0, double stoplimit = 0
            , Expiration expiration = null, string comment = null)
        {
            TradeRequest req = new TradeRequest();
            req.Price = price;
            req.RequestId = requestId;
            req.OrderPrice = stoplimit;
            req.Digits = Symbols.GetInfo(symbol).Digits;
            req.Volume = (ulong)Math.Round(lots * 100000000, 0);
            req.Currency = symbol;
            req.OrderType = type;
            if (type == OrderType.Buy || type == OrderType.Sell)
            {
                req.DealTicket = ticket;
                req.TradeType = TradeType.ModifyDeal;
            }
            else
            {
                req.OrderTicket = ticket;
                req.TradeType = TradeType.ModifyOrder;
            }
            req.Comment = comment;
            req.StopLoss = sl;
            req.TakeProfit = tp;
            req.ExpertId = expertID;
            if (expiration != null)
            {
                req.ExpirationType = expiration.Type;
                if (expiration.DateTime != new DateTime())
                    req.ExpirationTime = ConvertTo.Long(expiration.DateTime);
            }
            new OrderSender(Connection).Send(req);
        }

        FillPolicy GetFP(string symbol, OrderType type)
        {
            bool pendingOrder = true;
            if (type == OrderType.Buy || type == OrderType.Sell || type == OrderType.CloseBy)
                pendingOrder = false;
            FillPolicy fp;
            var group = Symbols.GetGroup(symbol);
            if (group.TradeType == ExecutionType.Request || group.TradeType == ExecutionType.Instant)
            {
                if (pendingOrder)
                    fp = FillPolicy.FlashFill;
                else
                    fp = FillPolicy.FillOrKill;
            }
            else
            {
                if (pendingOrder)
                    fp = FillPolicy.FlashFill;
                else if ((group.FillPolicy & FillingFlags.IOC) != 0)
                    fp = FillPolicy.ImmediateOrCancel;
                else
                   fp = FillPolicy.FillOrKill; // FOK or ANY
            }
            return fp;
        }

        private void EnsureTradingAllowed(string symbol, TradeAction action)
        {
            if (Account == null)
                throw new Exception("Account not loaded");

            // Investor = read-only
            if (IsInvestor)
                throw new InvestorModeException("Trading is disabled: account is in Investor (read-only) mode");
            if (IsTradeDisableOnServer)
                throw new TradeDiabledException("Trading is disabled for this account");

            var info = Symbols.GetInfo(symbol);
            var grp = Symbols.GetGroup(symbol);

            if (info == null)
                throw new InvalidSymbolException($"Symbol not found: {symbol}");

            // ===== SYMBOL TRADE MODE CHECK =====
            switch (grp.TradeMode)
            {
                case TradeMode.Disabled:
                    throw new TradeDiabledException($"Trading is disabled for symbol {symbol}");

                case TradeMode.CloseOnly:
                    if (action != TradeAction.Close)
                        throw new TradeDiabledException($"Trading is close-only for {symbol}");
                    break;

                case TradeMode.LongOnly:
                    if (action == TradeAction.Sell)
                        throw new TradeDiabledException($"Sell operations are forbidden for {symbol} (Long-Only mode)");
                    break;

                case TradeMode.ShortOnly:
                    if (action == TradeAction.Buy)
                        throw new TradeDiabledException($"Buy operations are forbidden for {symbol} (Short-Only mode)");
                    break;

                case TradeMode.FullAccess:
                default:
                    break; // trading allowed
            }
        }


        internal enum TradeAction
        {
            Buy,
            Sell,
            Close
        }
    }
}
