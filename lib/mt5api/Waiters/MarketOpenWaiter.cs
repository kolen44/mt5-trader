using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{

    class MarketOpenWaiter
    {
        MT5API Client;
        readonly int Id;
        readonly int Timeout;
        ConcurrentBag<OrderProgress> Progr = new ConcurrentBag<OrderProgress>();
        ConcurrentBag<OrderUpdate> Updates = new ConcurrentBag<OrderUpdate>();
        Order Order;
        long Ticket;

        public MarketOpenWaiter(MT5API client, int id, int timeout)
        {
            Client = client;
            Id = id;
            Timeout = timeout;
            Client.ProgressWaiters.TryAdd(Client_OnOrderProgress, 0);
            Client.UpdateWaiters.TryAdd(Client_OnOrderUpdate, 0);
        }

        private void Client_OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.Deal != null)
                if (Ticket == 0)
                    Updates.Add(update);
                else if (update.Deal.OrderTicket == Ticket)
                    Order = new Order(new DealInternal[] { update.Deal }, Id, Client);
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
                    if (Progr == null)
                        continue;
                    foreach (var progr in Progr)
                    {
                        var status = progr.TradeResult.Status;
                        if (status != Msg.REQUEST_ACCEPTED && status != Msg.REQUEST_ON_WAY && status != Msg.REQUEST_EXECUTED
                        && status != Msg.DONE && status != Msg.ORDER_PLACED)
                            throw new ServerException(status);
                    }
                    foreach (var progr in Progr)
                        if (progr.TradeResult.TicketNumber != 0)
                        {
                            Ticket = progr.TradeResult.TicketNumber;
                            foreach (var update in Updates)
                                if (update.Deal.PositionTicket == Ticket || update.Deal.OrderTicket == Ticket)
                                    Order = new Order(new DealInternal[] { update.Deal }, Id, Client); ;
                        }
                    if (Order != null)
                    {
                        Order.RequestId = Id;
                        if (Client.AccountMethod == AccMethod.Hedging)
                            Client.Orders.Opened.TryAdd(Order.Ticket, Order);
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
