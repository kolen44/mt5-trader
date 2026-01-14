using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{

    class PendingCloseWaiter
    {
        MT5API Client;
        readonly int Id;
        readonly int Timeout;
        OrderProgress Progr;
        Order Order;
        long Ticket;

        public PendingCloseWaiter(MT5API client, int id, int timeout, long ticket)
        {
            Client = client;
            Id = id;
            Timeout = timeout;
            Ticket = ticket;
            Client.ProgressWaiters.TryAdd(Client_OnOrderProgress, 0);
            Client.UpdateWaiters.TryAdd(Client_OnOrderUpdate, 0);
        }

        private void Client_OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            if (update.OrderInternal != null)
                   if (update.OrderInternal.TicketNumber == Ticket)
                    if (update.OrderInternal.State == OrderState.Cancelled)
                        Order = new Order(update.OrderInternal, Client);
        }

        private void Client_OnOrderProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId == Id)
                Progr = progress;
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
                    var status = Progr.TradeResult.Status;
                    if (status != Msg.REQUEST_ACCEPTED && status != Msg.REQUEST_ON_WAY && status != Msg.REQUEST_EXECUTED
                        && status != Msg.DONE && status != Msg.ORDER_PLACED)
                        throw new ServerException(status);
                    if (Order != null)
                    {
                        Order.RequestId = Id;
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
