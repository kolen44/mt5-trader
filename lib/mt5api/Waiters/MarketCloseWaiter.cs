using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{

    class MarketCloseWaiter
    {
        MT5API Client;
        readonly int Id;
        readonly int Timeout;
        ConcurrentBag<OrderProgress> Progr = new ConcurrentBag<OrderProgress>();
        Order Order;
        long Ticket;
        double ClosePrice;

        public MarketCloseWaiter(MT5API client, int id, int timeout, long ticket, bool notPartialClose, double closePrice)
        {
            Client = client;
            Id = id;
            Timeout = timeout;
            ClosePrice = closePrice;
            Client.ProgressWaiters.TryAdd(Client_OnOrderProgress, 0);
            Client.UpdateWaiters.TryAdd(Client_OnOrderUpdate, 0);
            Ticket = ticket;
        }

        private void Client_OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.Deal != null)
                if (update.Deal.PositionTicket == Ticket)
                    Order = new Order(new DealInternal[] { update.Deal, update.OppositeDeal }, Id, Client);
        }

        private void Client_OnOrderProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId == Id)
                Progr.Add(progress);
        }

        public Order Wait()
        {
            try
            {
                DateTime start = DateTime.Now;
                while (true)
                {
                    if (DateTime.Now.Subtract(start).TotalMilliseconds > Timeout)
                        throw new TradeTimeoutException("Trade timeout");
                    foreach (var progr in Progr)
                    {
                        var status = progr.TradeResult.Status;
                        if (status != Msg.REQUEST_ACCEPTED && status != Msg.REQUEST_ON_WAY && status != Msg.REQUEST_EXECUTED
                            && status != Msg.DONE && status != Msg.ORDER_PLACED)
                            throw new ServerException(status);
                    }
                    if (Order != null)
                    {

                        Order.RequestId = Id;
                        var ticket = Order.Ticket;
                        //lock (Client.Orders.Opened)
                        //{
                        //    Order order;
                        //    if (Client.Orders.Opened.ContainsKey(ticket))
                        //    {
                        //        order = Client.Orders.Opened[ticket];
                        //        order.Update(Order);
                        //        if(NotPartialClose)
                        //            Client.Orders.Opened.Remove(ticket);
                        //    }
                        //    else
                        //        order = Order;
                        //    lock (Client.Orders.Closed)
                        //        if (!Client.Orders.Closed.ContainsKey(ticket))
                        //            Client.Orders.Closed.Add(ticket, Order);
                        //        else
                        //            Client.Orders.Closed[ticket].Update(Order);
                        //}
                        return Order;
                    }
                    Thread.Sleep(1);
                }
            }
            finally
            {
                Client.ProgressWaiters.TryRemove(Client_OnOrderProgress, out var _);
                Client.UpdateWaiters.TryRemove(Client_OnOrderUpdate, out var _);
            }
        }
    }
}
