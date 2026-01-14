using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace mtapi.mt5
{
    class ModifyWaiter
    {
        MT5API Client;
        readonly int Id;
        readonly int Timeout;
        ConcurrentBag<OrderProgress> Progr = new ConcurrentBag<OrderProgress>();

        public ModifyWaiter(MT5API client, int id, int timeout)
        {
            Client = client;
            Id = id;
            Timeout = timeout;
            Client.ProgressWaiters.TryAdd(Client_OnOrderProgress, 0);
        }

        private void Client_OnOrderProgress(MT5API sender, OrderProgress progress)
        {
            if (progress.TradeRequest.RequestId == Id)
                Progr.Add(progress);
        }

        public void Wait()
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
                        if (status == Msg.REQUEST_EXECUTED)
                            return;
                    }
                    Thread.Sleep(1);
                }
            }
            finally
            {
                Client.ProgressWaiters.TryRemove(Client_OnOrderProgress, out var _);
            }
        }
    }
}
