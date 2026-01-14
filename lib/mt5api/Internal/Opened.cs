using mtapi.mt5.Struct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    class OpenedClosedOrders
    {
        Logger Log;
        readonly MT5API Api;

        internal ConcurrentDictionary<long, Order> Opened = new ConcurrentDictionary<long, Order>();
        internal ConcurrentDictionary<long, Order> Closed = new ConcurrentDictionary<long, Order>();
        internal ConcurrentDictionary<long, byte> TicketsStopLoss = new ConcurrentDictionary<long, byte>();
        internal ConcurrentDictionary<long, byte> TicketsTakeProfit = new ConcurrentDictionary<long, byte>();

        internal OpenedClosedOrders(MT5API api, Logger log)
        {
            Api = api;
            Log = log;
        }

        string Proc;

        internal void Api_OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            Proc = "0 ";
            try
            {
                Process(sender, update);
                Proc += "36 ";
            }
            catch (Exception ex)
            {
                Log.warn(ex, Api);
            }
        }
        int num = 0;

        LinkedList<OrderUpdate> OrderUpdates = new LinkedList<OrderUpdate>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        void Process(MT5API sender, OrderUpdate update)
        {
            if (sender == null)
                return;
            if (update == null)
                return;
            foreach (var item in OrderUpdates)
                if (OrderUpdate.AreEqual(item, update))
                    return;
            OrderUpdates.AddLast(update);
            if (OrderUpdates.Count > 100)
                OrderUpdates.RemoveFirst();
            Proc += "1 ";
            var updateOrderInternal = update.OrderInternal;
            if (updateOrderInternal != null)
            {
                
                if (updateOrderInternal.State == OrderState.Placed)
                {
                    Proc += "2 ";
                    if (updateOrderInternal.DealTicket == 0)
                    {
                        Order order;
                        if (Opened.TryGetValue(updateOrderInternal.TicketNumber, out order))
                        {
                            Proc += "3 ";
                            order.Update(new Order(updateOrderInternal, sender));
                            update.Type = UpdateType.PendingModify;
                        }
                        else
                        {
                            Proc += "4 ";
                            order = new Order(updateOrderInternal, sender);
                            if (order.OrderType != OrderType.Sell && order.OrderType != OrderType.Buy)
                                Opened.TryAdd(updateOrderInternal.TicketNumber, order);
                            update.Type = UpdateType.PendingOpen;
                        }
                        update.Order = order;
                    }
                }
                else if (updateOrderInternal.State == OrderState.Cancelled
                    || updateOrderInternal.State == OrderState.Expired
                    || updateOrderInternal.State == OrderState.Rejected)
                {
                    Proc += "5 ";
                    if (Opened.TryGetValue(updateOrderInternal.TicketNumber, out var order))
                    {
                        Proc += "6 ";
                        order.Update(new Order(updateOrderInternal, sender));
                        Closed.TryAdd(updateOrderInternal.TicketNumber, order);
                        while (Closed.Count > 100)
                            Closed.TryRemove(Closed.Keys.Min(), out var tmp);
                        Opened.TryRemove(updateOrderInternal.TicketNumber, out var tmp2);
                        update.Order = order;
                    }
                    else
                        update.Order = new Order(updateOrderInternal, sender);
                    Proc += "7 ";
                    if (updateOrderInternal.State == OrderState.Expired)
                        update.Type = UpdateType.Expired;
                    else if (updateOrderInternal.State == OrderState.Rejected)
                        update.Type = UpdateType.Rejected;
                    else
                        update.Type = UpdateType.PendingClose;
                }
                else if (updateOrderInternal.State == OrderState.Started)
                {
                    Proc += "8 ";
                    update.Order = new Order(updateOrderInternal, sender);
                    update.Type = UpdateType.Started;
                }
                else if (updateOrderInternal.State == OrderState.Filled)
                {
                    Proc += "9 ";
                    if (Opened.TryGetValue(updateOrderInternal.DealTicket, out var openedOrder))
                        if (updateOrderInternal.PlacedType == PlacedType.OnSL)
                            TicketsStopLoss.TryAdd(updateOrderInternal.DealTicket, 0);
                        else if (updateOrderInternal.PlacedType == PlacedType.OnTP)
                            TicketsTakeProfit.TryAdd(updateOrderInternal.DealTicket, 0);
                    if (Opened.TryGetValue(updateOrderInternal.TicketNumber, out var order))
                        update.Order = order;
                    else
                        update.Order = new Order(updateOrderInternal, sender);
                    update.Type = UpdateType.Filled;
                }
                else if (updateOrderInternal.State == OrderState.RequestCancelling)
                {
                    Proc += "10 ";
                    if (Opened.TryGetValue(updateOrderInternal.TicketNumber, out var order))
                        update.Order = order;
                    else
                        update.Order = new Order(updateOrderInternal, sender);
                    update.Type = UpdateType.Cancelling;
                }
            }
            var updateDeal = update.Deal;
            if (updateDeal != null)
            {
                Proc += "11 ";
                if(updateDeal.PlacedType == PlacedType.OnSL)
                {
                    if (Opened.TryGetValue(updateDeal.PositionTicket, out var openedOrder))
                    {
                        TicketsStopLoss.TryRemove(updateDeal.PositionTicket, out _);
                        openedOrder.UpdateOnStop(updateDeal, false);
                        update.Order = openedOrder;
                        update.Type = UpdateType.OnStopLoss;
                        Closed.TryAdd(updateDeal.PositionTicket, update.Order);
                        while (Closed.Count > 100)
                            Closed.TryRemove(Closed.Keys.Min(), out _);
                        Opened.TryRemove(updateDeal.PositionTicket, out _);
                    }
                }
                else if (updateDeal.PlacedType == PlacedType.OnTP)
                {
                    if (Opened.TryGetValue(updateDeal.PositionTicket, out var openedOrder))
                    {
                        TicketsTakeProfit.TryRemove(updateDeal.PositionTicket, out _);
                        openedOrder.UpdateOnStop(updateDeal, false);
                        update.Order = openedOrder;
                        update.Type = UpdateType.OnTakeProfit;
                        Closed.TryAdd(updateDeal.PositionTicket, update.Order);
                        while (Closed.Count > 100)
                            Closed.TryRemove(Closed.Keys.Min(), out _);
                        Opened.TryRemove(updateDeal.PositionTicket, out _);
                    }
                }
                if (updateDeal.Type == DealType.Balance)
                {
                    Proc += "12 ";
                    var ticket = updateDeal.TicketNumber;
                    if (Closed.ContainsKey(ticket)) //double message for deposit/withdraw 
                        return;
                    foreach (var item in Closed.Values)
                        if (item.DealType == DealType.Balance && item.Profit == updateDeal.Profit)
                            if (Math.Abs(updateDeal.OpenTimeAsDateTime.Subtract(item.OpenTime).TotalSeconds) < 3)
                                return; //double message for deposit/withdraw 
                    update.Order = new Order(updateDeal, sender);
                    update.Type = UpdateType.Balance;
                    Closed.TryAdd(ticket, update.Order);
                    while (Closed.Count > 100)
                        Closed.TryRemove(Closed.Keys.Min(), out _);
                }
                else
                {
                    Proc += "14 ";
                    var ticket = updateDeal.PositionTicket;
                    long closeByTicket = 0;
                    if (updateDeal.Direction == Direction.OutBy)
                        if (updateDeal.Comment != null)
                            if (updateDeal.Comment.Contains("by #"))
                                closeByTicket = long.Parse(updateDeal.Comment.Substring(updateDeal.Comment.IndexOf("by #") + "by #".Length));
                    var updateOppositeDeal = update.OppositeDeal;
                    if (ticket == 0)
                    {
                        Proc += "15 ";
                        if (updateOppositeDeal != null)
                        {
                            ticket = updateOppositeDeal.PositionTicket;
                            int digits = 5;
                            if (Api.Symbols != null)
                            {
                                var info = Api.Symbols.GetInfo(updateOppositeDeal.Symbol);
                                if(info != null)
                                    digits = info.Digits;
                            }
                            if (updateDeal.OpenTimeMs != updateOppositeDeal.OpenTimeMs)
                            {
                                Proc += "16 ";
                                if (Opened.TryGetValue(ticket, out var openedOrder))
                                {
                                    Proc += "17 ";
                                    if (updateOppositeDeal.StopLoss > 0 && TicketsStopLoss.ContainsKey(updateOppositeDeal.PositionTicket)) 
                                    {
                                        Proc += "18 ";
                                        TicketsStopLoss.TryRemove(updateOppositeDeal.PositionTicket, out _);
                                        openedOrder.UpdateOnStop(updateOppositeDeal, true);
                                        update.Order = openedOrder;
                                        update.Type = UpdateType.OnStopLoss;
                                        Closed.TryAdd(ticket, update.Order);
                                        while (Closed.Count > 100)
                                            Closed.TryRemove(Closed.Keys.Min(), out _);
                                        Opened.TryRemove(ticket, out _);
                                    }
                                    else if (updateOppositeDeal.TakeProfit > 0 && TicketsTakeProfit.ContainsKey(updateOppositeDeal.PositionTicket))
                                    {
                                        Proc += "19 ";
                                        TicketsTakeProfit.TryRemove(updateOppositeDeal.PositionTicket, out _);
                                        openedOrder.UpdateOnStop(updateOppositeDeal, true);
                                        update.Order = openedOrder;
                                        update.Type = UpdateType.OnTakeProfit;
                                        Closed.TryAdd(ticket, update.Order);
                                        while (Closed.Count > 100)
                                            Closed.TryRemove(Closed.Keys.Min(), out _);
                                        Opened.TryRemove(ticket, out _);
                                    }
                                    else
                                    {
                                        Proc += "20 ";
                                        if (!openedOrder.Update(new Order(updateOppositeDeal, sender)))
                                            return;
                                        update.Order = openedOrder;
                                        update.Type = UpdateType.MarketModify;
                                    }
                                }
                            }
                        }
                    }
                    else if (updateDeal.OpenTimeMs == updateOppositeDeal?.OpenTimeMs && Api.AccountMethod == AccMethod.Hedging)
                    {
                        Proc += "21 ";
                        update.Type = UpdateType.MarketOpen;
                        if (Opened.TryGetValue(ticket, out var order))
                        {
                            //var lotsBefore = order.Lots;
                            order.Update(new Order(updateDeal, sender));
                            //if(order.Lots != lotsBefore)
                            //    update.Type = UpdateType.PartialFill;
                        }
                        else
                        {
                            order = new Order(updateDeal, sender);
                            Opened.TryAdd(ticket, order);
                        }
                        update.Order = order;

                    }
                    else
                    {
                        Proc += "23 ";
                        if (Api.AccountMethod == AccMethod.Netting || Api.AccountMethod == AccMethod.Default)
                        {
                            if (updateOppositeDeal != null)
                            {
                                int digits = 5;
                                if (Api.Symbols != null)
                                {
                                    var info = Api.Symbols.GetInfo(updateOppositeDeal.Symbol);
                                    if (info != null)
                                        digits = info.Digits;
                                }
                                long nettingTicket = 0;
                                foreach (var item in Opened.Values)
                                    if (item.Symbol == updateDeal.Symbol)
                                        if (item.OrderType == OrderType.Buy || item.OrderType == OrderType.Sell)
                                        {
                                            nettingTicket = item.Ticket;
                                            break;
                                        }
                                if (nettingTicket > 0 && Opened.ContainsKey(nettingTicket)) //in case of exvents not sorted by time
                                {
                                    Proc += "27 ";
                                    //if (nettingTicket <= updateOppositeDeal.TicketNumber)
                                    //{
                                    var order = Opened[nettingTicket];
                                    if (order != null)
                                    {
                                        var open = order.Clone();
                                        order.Update(new Order(updateOppositeDeal, sender));
                                        order.Ticket = updateOppositeDeal.TicketNumber;
                                        Opened.TryRemove(nettingTicket, out _);
                                        Opened.TryRemove(updateDeal.OrderTicket, out _);

                                        if (updateOppositeDeal.Volume > 0)
                                        {
                                            Opened.TryAdd(order.Ticket, order);
                                            update.Type = UpdateType.MarketOpen;
                                            update.Order = new Order(updateDeal, sender);
                                        }
                                        else
                                        {
                                            Proc += "28 ";
                                            update.Type = UpdateType.MarketClose;
                                            if (Closed.TryGetValue(ticket, out var value))
                                                value.Update(open);
                                            else
                                            {
                                                Closed.TryAdd(ticket, open);
                                                while (Closed.Count > 100)
                                                    Closed.TryRemove(Closed.Keys.Min(), out var tmp2);
                                            }
                                            update.Order = new Order(new DealInternal[] { updateDeal, updateOppositeDeal }, sender);
                                        }
                                    }
                                    //}
                                }
                                else if (Closed.ContainsKey(updateOppositeDeal.TicketNumber))
                                {
                                    // stop loss or take profit, order deleted already
                                }
                                else
                                {
                                    var order = new Order(updateDeal, sender);
                                    if (Opened.TryGetValue(ticket, out var old))
                                        old.Update(order);
                                    else
                                        Opened.TryAdd(ticket, order);
                                    update.Type = UpdateType.MarketOpen;
                                    update.Order = order;
                                }
                            }
                        }
                        else if (Opened.TryGetValue(ticket, out var open))
                        {
                            Proc += "24 ";
                            if (updateDeal.PlacedType == PlacedType.OnStopOut ||
                                    (
                                        updateDeal.Direction == Direction.Out &&
                                        (updateDeal.PlacedType == PlacedType.OnSL || updateDeal.PlacedType == PlacedType.OnTP)
                                    )
                                )
                            {
                                Proc += "25 ";
                                if (updateDeal.PlacedType == PlacedType.OnSL)
                                    update.Type = UpdateType.OnStopLoss;
                                else if (updateDeal.PlacedType == PlacedType.OnTP)
                                    update.Type = UpdateType.OnTakeProfit;
                                else
                                    update.Type = UpdateType.OnStopOut;
                                open.UpdateOnStop(updateDeal);
                                Closed.TryAdd(ticket, open);
                                while (Closed.Count > 100)
                                    Closed.TryRemove(Closed.Keys.Min(), out var tmp2);
                                Opened.TryRemove(ticket, out var tmp);
                                update.Order = open;
                                update.Order.State = OrderState.Filled;
                            }
                            else if (updateDeal.Direction == Direction.In)
                            {

                            }
                            else
                            {
                                Proc += "26 ";
                                var closeLots = new Order(updateDeal, sender).Lots;
                                if (Math.Round(open.Lots, 8) == closeLots)
                                {
                                    open.Lots = 0;
                                    Opened.TryRemove(ticket, out _);
                                    update.Type = UpdateType.MarketClose;
                                    open.Update(updateDeal);
                                    open.Swap = updateDeal.Swap;
                                }
                                else
                                {
                                    open.Lots -= closeLots;
                                    update.Type = UpdateType.PartialClose;
                                    open.Update(updateDeal);
                                    if (updateOppositeDeal != null)
                                    {
                                        open.Profit = updateOppositeDeal.Profit;
                                        open.Swap = updateOppositeDeal.Swap;
                                    }
                                    var partialCloseDeails = new List<DealInternal>(open.PartialCloseDeals);
                                    partialCloseDeails.Add(updateDeal);
                                    open.PartialCloseDeals = partialCloseDeails.ToArray();
                                }

                                update.Order = open;
                                if (update.Type == UpdateType.MarketClose)
                                    update.Order.Profit = update.Order.CloseProfit;
                                if (closeByTicket > 0)
                                {
                                    update.Type = UpdateType.MarketCloseBy;
                                    if (Opened.TryGetValue(closeByTicket, out var closeByOrder))
                                    {
                                        if (closeByOrder.Lots == closeLots)
                                            Opened.TryRemove(closeByTicket, out _);
                                        else if (closeByOrder.Lots > closeLots)
                                            closeByOrder.Lots -= closeLots;
                                    }
                                    update.CloseByTicket = closeByTicket;
                                }
                                Proc += "30 ";
                                if (Closed.TryGetValue(ticket, out var value))
                                    value.Update(open);
                                else
                                {
                                    Closed.TryAdd(ticket, open);
                                    while (Closed.Count > 100)
                                        Closed.TryRemove(Closed.Keys.Min(), out var tmp2);
                                }
                            }
                        }
                        else if (updateOppositeDeal != null && Opened.TryGetValue(updateOppositeDeal.PositionTicket, out var order))
                        {
                            Proc += "32 ";
                            order.Update(new Order(updateOppositeDeal, sender));
                            update.Order = order;
                            update.Type = UpdateType.MarketModify;
                        }
                        else if (updateOppositeDeal != null && (Api.AccountMethod == AccMethod.Netting || Api.AccountMethod == AccMethod.Default))
                            Opened.TryAdd(ticket, new Order(updateOppositeDeal, sender));
                        else if (Api.AccountMethod == AccMethod.Hedging && updateDeal.Direction == Direction.In && updateDeal.OpenTimeMs > 0)
                        {
                            Proc += "21 ";
                            update.Type = UpdateType.MarketOpen;
                            if (Opened.TryGetValue(ticket, out var o))
                            {
                                //var lotsBefore = order.Lots;
                                o.Update(new Order(updateDeal, sender));
                                //if(order.Lots != lotsBefore)
                                //    update.Type = UpdateType.PartialFill;
                                update.Order = o;
                            }
                            else
                            {
                                order = new Order(updateDeal, sender);
                                Opened.TryAdd(ticket, order);
                                update.Order = order;
                            }
                            
                        }
                    }
                }
                if (updateDeal.Type == DealType.Credit)
                    Api.Account.Credit += updateDeal.Profit + updateDeal.Swap + updateDeal.Commission;
                else
                    Api.Account.Balance += updateDeal.Profit + updateDeal.Swap + updateDeal.Commission;
            }
            Proc += "33 ";
            var updateOrder = update.Order;
            if (updateOrder != null)
            {
                if (Math.Round(updateOrder.Commission, 8) == Math.Round(updateOrder.OpenPrice, 8))
                    updateOrder.Commission = 0;
                update.Order = updateOrder.Clone(); //to avoid changes by further events during processing
            }
            Proc += "34 ";
        }

        bool AreAlmostEqual(double a, double b, int digits)
        {
            if (digits <= 0)
            {
                // Integer comparison: remove youngest digit
                long aInt = (long)a;
                long bInt = (long)b;
                return (aInt / 10) == (bInt / 10);
            }

            double scale = Math.Pow(10, digits - 1);
            long aScaled = (long)(a * scale);
            long bScaled = (long)(b * scale);
            return aScaled == bScaled;
        }

        internal void Add(List<DealInternal> deals, MT5API api)
        {
            Add(deals, api, Opened);
        }

        internal static void Add(List<DealInternal> deals, MT5API api, ConcurrentDictionary<long, Order> dic)
        {
            foreach (var item in deals)
            {
                var order = new Order(item, api);
                dic.TryAdd(item.TicketNumber, order);
                if(api.Connection != null)
                    UpdateClosePriceAsync(order, api);
            }
        }

        static void UpdateClosePriceAsync(Order order, MT5API api)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (order.OrderType == OrderType.Buy)
                        order.ClosePrice = (await api.GetQuoteAsync(order.Symbol, 5000)).Bid;
                    else if (order.OrderType == OrderType.Sell)
                        order.ClosePrice = (await api.GetQuoteAsync(order.Symbol, 5000)).Ask;
                }
                catch (Exception)
                {
                }
            });
        }

        internal void Add(List<OrderInternal> orders, MT5API api)
        {
            Add(orders, api, Opened);
        }

        internal static void Add(List<OrderInternal> orders, MT5API api, ConcurrentDictionary<long, Order> dic)
        {
            foreach (var item in orders)
                dic.TryAdd(item.TicketNumber, new Order(item, api));
        }

        //internal Order[] GetOpenedOrders()
        //{
        //    Log.trace("GetOpenedOrders");
        //    if (Orders == null || Deals == null)
        //        throw new Exception("Opened orders and deals not loaded yet");
        //    List<Order> res = new List<Order>();
        //    foreach (var item in Orders)
        //        res.Add(new Order(item));
        //    foreach (var item in Deals)
        //        res.Add(new Order(item));
        //    return res.ToArray();
        //}

        internal bool ParseTrades(InBuf buf)
        {
            var cmd = buf.Byte();
            switch (cmd)
            {
                case 7:                                     //symbol configuration
                    //UpdateSymbols(buf);
                    return true;
                case 8:                                     //symbol group configuration
                    //UpdateSymbolSet(buf);
                    return false;
                case 9:                                     //group configuration
                    //UpdateSymbolsBase(buf);
                    return false;
                case 0x11:                                  //tickers
                    UpdateTickers(buf);
                    break;
                case 0x13:                                  //users
                    UpdateAccount(buf);
                    break;
                case 0x1F:                                  //orders
                    UpdateOrders(buf);
                    break;
                case 0x20:                                  //history of orders
                    UpdateHistoryOrders(buf);
                    break;
                case 0x21:                                  //all deals
                    UpdateDeals(buf);
                    break;
                case 0x23:                                  //request
                    UpdateTradeRequest(buf);
                    break;
                case 0x28:                                  //spread config
                    UpdateSpreads(buf);
                    break;
                default:
                    cmd.ToString();
                    break;
            }
            return false;
        }

        private void UpdateSpreads(InBuf buf)
        {
            Log.trace("UpdateSpreads");
        }

        ConcurrentDictionary<long, int> ReqId = new ConcurrentDictionary<long, int>();

        private void UpdateTradeRequest(InBuf buf)
        {
            Log.trace("UpdateTradeRequest");
            if (buf == null)
                throw new ArgumentNullException("UpdateTradeRequest: buf is null");
            int num = buf.Int();
            OrderProgress[] array = new OrderProgress[num];
            int count = buf.Left / num - 1212; // size TransactionInfo + TradeRequest + TradeResult 476
            for (int i = 0; i < num; i++)
            {
                OrderProgress progress = new OrderProgress();
                progress.OrderUpdate = buf.Struct<TransactionInfo>();
                if (Api.Connection != null)
                    if (Api.Connection.TradeBuild <= 1891)
                        throw new NotImplementedException("TradeBuild <= 1891");
                progress.TradeRequest = buf.Struct<TradeRequest>();
                progress.TradeResult = buf.Struct<TradeResult>();
                //progress.DealsResult = buf.Struct<DealsResult>();
                buf.Bytes(count);
                array[i] = progress;
            }
            var res = new LinkedList<OrderProgress>();
            foreach (var item in array)
            {
                if (item.TradeRequest.RequestId != 0 && item.TradeResult.TicketNumber != 0)
                    ReqId.TryAdd(item.TradeResult.TicketNumber, item.TradeRequest.RequestId);
                if (item.TradeResult.Status == Msg.REQUEST_EXECUTED)
                {
                    if (item.TradeRequest.RequestId == 0)
                        if (ReqId.TryGetValue(item.TradeResult.TicketNumber, out var id))
                        {
                            item.TradeRequest.RequestId = id;
                            ReqId.TryRemove(item.TradeResult.TicketNumber, out var tmp);
                        }
                    res.AddLast(item);
                }
                else
                    res.AddLast(item);
            }
            Api.OnOrderProgressCall(res.ToArray());
        }

        private void UpdateDeals(InBuf buf)
        {
            Log.trace("UpdateDeals");
            int num = buf.Int();
            var ar = new OrderUpdate[num];
            if (Api.Connection != null)
                if (Api.Connection.TradeBuild <= 1892)
                    throw new NotImplementedException("TradeBuild <= 1892");
            for (int i = 0; i < num; i++)
            {
                var ou = new OrderUpdate();
                ou.Trans = buf.Struct<TransactionInfo>();
                ou.Deal = buf.Struct<DealInternal>();
                ou.OppositeDeal = buf.Struct<DealInternal>();
                var s5 = buf.Struct<PumpDeals5D8>();
                var s6 = buf.Struct<PumpDeals698>();
                if (Api.Connection.TradeBuild <= 1241)
                    continue;
                var deals = buf.Array<DealInternal>();
                var opposite = buf.Array<DealInternal>();
                ar[i] = ou;
            }
            Api.OnOrderUpdateCall(ar);
        }

        private void UpdateHistoryOrders(InBuf buf)
        {
            Log.trace("UpdateHistoryOrders");
        }

        private void UpdateOrders(InBuf buf)
        {
            Log.trace("UpdateOrders");
            while (Api.Connection == null)
                Thread.Sleep(10);
            int num = buf.Int();
            var ar = new OrderUpdate[num];
            for (int i = 0; i < num; i++)
            {
                if (Api.Connection.TradeBuild <= 1891)
                    throw new NotImplementedException("TradeBuild <= 1891");
                var ou = new OrderUpdate();
                ou.Trans = buf.Struct<TransactionInfo>();
                ou.OrderInternal = buf.Struct<OrderInternal>();
                ar[i] = ou;
            }
            Api.OnOrderUpdateCall(ar);
        }

        private void UpdateAccount(InBuf buf)
        {
            Log.trace("UpdateSymbolSet");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                buf.Bytes(0xD8);
                Api.Account = UDT.ReadStruct<AccountRec>(buf);
            }
        }

        private void UpdateTickers(InBuf buf)
        {
            Log.trace("UpdateTickers");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                buf.Bytes(24);
                if (Api.Connection?.SymBuild <= 1036)
                    throw new NotImplementedException();
                else
                {
                    var ticker = buf.Struct<Ticker>();
                }
            }
        }

        private void UpdateSymbolsBase(InBuf buf)
        {
            Log.trace("UpdateSymbolSet");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                var config = buf.Struct<SymbolConfig>();
                var sb = UDT.ReadStruct<SymBaseInfo>(buf);
                switch (config.Action)
                {
                    case UpdateAction.Add:
                    case UpdateAction.Update:
                    case UpdateAction.Delete:
                        break;
                }
            }
            if (Api.Connection?.SymBuild > 4072)
                AccountLoader.LoadLeveargeBase(buf);
        }

        private void UpdateSymbolSet(InBuf buf)
        {
            Log.trace("UpdateSymbolSet");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                var config = buf.Struct<SymbolConfig>();
                var ss = UDT.ReadStruct<SymbolSet>(buf);
                switch (config.Action)
                {
                    case UpdateAction.Add:
                    case UpdateAction.Update:
                    case UpdateAction.Delete:
                        break;
                }
            }
        }

        private void UpdateSymbols(InBuf buf)
        {
            Log.trace("UpdateSymbols");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                var config = buf.Struct<SymbolConfig>();
                var si = UDT.ReadStruct<SymbolInfo>(buf);
                var gr = UDT.ReadStruct<SymGroup>(buf);
                var sessions = AccountLoader.LoadSessions(buf);
                var symTicks = UDT.ReadStruct<SymTicks>(buf);
                switch (config.Action)
                {
                    case UpdateAction.Add:
                    case UpdateAction.Update:
                        Api.Symbols.Infos[si.Currency] = si;
                        Api.Symbols.InfosById[si.Id] = si;
                        Api.Symbols.Groups[si.Currency] = gr;
                        Api.Symbols.Sessions[si.Currency] = sessions;
                        break;
                    case UpdateAction.Delete:
                        Api.Symbols.Infos.TryRemove(si.Currency, out _);
                        Api.Symbols.InfosById.TryRemove(si.Id, out _);
                        Api.Symbols.Groups.TryRemove(si.Currency, out _);
                        Api.Symbols.Sessions.TryRemove(si.Currency, out _);
                        break;
                }
                Api.OnSymbolUpdateCall(new SymbolUpdate(si.Currency, gr, sessions, config.Action) { Ticks = symTicks });
            }
        }
    }
}
