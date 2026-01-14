using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class OrderHistory
    {
        Logger Log;
        readonly MT5API Api;
        //ConcurrentDictionary<long, ConcurrentDictionary<long, DealInternal>> ExistDeals = new ConcurrentDictionary<long, ConcurrentDictionary<long, DealInternal>>();

        internal OrderHistory(MT5API api, Logger log)
        {
            Log = log;
            Api = api;
        }

        internal async Task Request(DateTime from, DateTime to, DealInternal[] partialResponse, DealInternal[] exist)
        {
            int partialResponseCount = 0;
            long partialReponseMaxTime = 0;
            if (partialResponse != null)
            {
                partialResponseCount = partialResponse.Length;
                foreach (var item in partialResponse)
                    if (item.HistoryTime > partialReponseMaxTime)
                        partialReponseMaxTime = item.HistoryTime;
                foreach (var item in partialResponse)
                {
                    //ExistDeals.TryAdd(item.PositionTicket, new ConcurrentDictionary<long, DealInternal>());
                    //ExistDeals[item.PositionTicket].TryAdd(item.TicketNumber, item);
                }
            }
            if (exist != null)
                foreach (var item in exist)
                {
                    //ExistDeals.TryAdd(item.PositionTicket, new ConcurrentDictionary<long, DealInternal>());
                    //ExistDeals[item.PositionTicket].TryAdd(item.TicketNumber, item);
                }
            OutBuf buf = new OutBuf();
            buf.ByteToBuffer(0x21);
            buf.LongLongToBuffer(ConvertTo.Long(from));
            buf.LongLongToBuffer(ConvertTo.Long(to));
            if (partialResponseCount > 0)
            {
                buf.LongToBuffer(1);
                buf.LongLongToBuffer(ConvertTo.Long(from));// 1646092800);//ConvertTo.Long(existFrom));
                buf.LongLongToBuffer(ConvertTo.Long(to));// 1648771199);//ConvertTo.Long(existTo));
                buf.LongLongToBuffer(partialReponseMaxTime);
                buf.LongToBuffer((uint)partialResponseCount);
                buf.LongToBuffer(0);
                buf.LongToBuffer(0);
                buf.LongToBuffer(0);
            }
            else
                buf.LongToBuffer(0);
            await Api.Connection.SendPacket(0x65, buf);
        }

        internal async Task RequestPending(DateTime from, DateTime to, int existCount = 0, long historyTime = 0)
        {
            OutBuf buf = new OutBuf();
            buf.ByteToBuffer(0x20);
            buf.LongLongToBuffer(ConvertTo.Long(from));
            buf.LongLongToBuffer(ConvertTo.Long(to));
            if (existCount > 0)
            {
                buf.LongToBuffer(1);
                buf.LongLongToBuffer(ConvertTo.Long(from));
                buf.LongLongToBuffer(ConvertTo.Long(to));
                buf.LongLongToBuffer(historyTime);
                buf.LongToBuffer((uint)existCount);
                buf.LongToBuffer(0);
                buf.LongToBuffer(0);
                buf.LongToBuffer(0);
            }
            else
                buf.LongToBuffer(0);
            await Api.Connection.SendPacket(0x65, buf);
        }

        internal void Parse(InBuf buf)
        {
            byte cmd = buf.Byte();
            if (cmd == 0x20)
            {
                var res = Parse<OrderInternal>(buf);
                var args = new OrderHistoryEventArgs();
                args.Orders = new List<Order>();
                args.InternalDeals = new List<DealInternal>();
                args.InternalOrders = res.ToList();
                Api.OnOrderHisotyCall(args);
            }
            else if (cmd == 0x21)
            {
                int action = 0;
                var res = ParseDeals(buf, ref action);
                //foreach (var position in res)
                //    if (ExistDeals.TryGetValue(position.Key, out var existDeals))
                //        foreach (var deal in existDeals.Values)
                //            position.Value.TryAdd(deal.TicketNumber, deal);

                List<Order> list = new List<Order>();
                foreach (var item in res)
                {
                    foreach (var deal in item.Value.Values)
                        if (deal.Type == DealType.Balance || deal.Type == DealType.Credit)
                            list.Add(new Order(deal, Api));

                    if (item.Value.Count > 1)
                    {
                        if (item.Key != 0 && item.Value.First().Value.Type != DealType.Balance) // balance
                            list.Add(new Order(item.Value.Values.ToArray(), Api));
                    }
                    if (item.Value.Count == 1)
                    {
                        var deal = item.Value.First().Value;
                        if (deal.Type != DealType.Balance && deal.Type != DealType.Credit)
                        {
                            if (deal.Type == DealType.DealBuy || deal.Type == DealType.DealSell)
                            {
                                if (deal.Direction == Direction.Out || deal.Direction == Direction.OutBy)
                                    list.Add(new Order(item.Value.Values.ToArray(), Api)); // add positions without opening deal
                            }
                            else
                                list.Add(new Order(deal, Api));
                        }
                    }
                }
                var args = new OrderHistoryEventArgs();
                args.Action = action;
                foreach (var item in list)
                    if (item.DealType == DealType.Balance)
                        item.CloseTime = item.OpenTime;
                args.Orders = list;
                var deals = new List<DealInternal>();
                foreach (var item in res)
                    deals.AddRange(item.Value.Values);
                args.InternalDeals = deals;
                args.InternalOrders = new List<OrderInternal>();
                Api.OnOrderHisotyCall(args);
            }
            else
                throw new Exception("Unknown Trade Parse Cmd = 0x" + cmd.ToString("X"));
        }

        private T[] Parse<T>(InBuf buf) where T : FromBufReader, new()
        {
            //buf.Int();
            int updId = buf.Int();
            int num = buf.Int();
            List<T> res = new List<T>();
            for (int i = 0; i < num; i++)
            {
                var time = ConvertTo.DateTime(buf.Long());
                int action = buf.Int();
                if (action == 1)
                    continue;
                if (action == 4)
                {
                    //RemoveItem(time);
                    continue;
                }
                //if ((action != 0) && (action != 0xE))
                //{
                long[] tickets;
                res.AddRange(ParseReceivedData<T>(action, buf, out tickets));
                //}
            }
            return res.ToArray();
        }

        private Dictionary<long, ConcurrentDictionary<long, DealInternal>> ParseDeals(InBuf buf, ref int action)
        {
            int updId = buf.Int();
            int num = buf.Int();
            var res = new Dictionary<long, ConcurrentDictionary<long, DealInternal>>();
            for (int i = 0; i < num; i++)
            {
                var time = ConvertTo.DateTime(buf.Long());
                action = buf.Int();
                if (action == 1)
                    continue;
                if (action == 4)
                {
                    //RemoveItem(time);
                    //continue;
                }
                if (res == null)
                    res = ParseReceivedDeals(action, buf);
                else
                    foreach (var x in ParseReceivedDeals(action, buf))
                        if (!res.ContainsKey(x.Key))
                            res.Add(x.Key, x.Value);
                        else
                            foreach (var item in x.Value)
                                res[x.Key].TryAdd(item.Key, item.Value);
            }
            return res;
        }

        private Dictionary<long, ConcurrentDictionary<long, DealInternal>> ParseReceivedDeals(int action, InBuf buf)
        {
            if (action == 0)
            {
                int num = buf.Int();
                var tickets = new long[num];
                for (int i = 0; i < num; i++)
                    tickets[i] = buf.Long();
                if (Api.Connection.TradeBuild <= 1892)
                    throw new NotImplementedException("TradeBuild <= 1892");
                var res = buf.ArrayDeal();
                buf.Bytes(16);
                return res;
            }
            else
            {
                Msg status = (Msg)buf.Int();
                var res = buf.ArrayDeal();
                return res;
            }
        }

        //private void ParseOrders(InBuf buf)
        //{
        //	int updId = buf.Int();
        //	int num = buf.Int();
        //	for (int i = 0; i < num; i++)
        //	{
        //		var time = ConvertTo.DateTime(buf.Long());
        //		int action = buf.Int();
        //		if (action == 1)
        //			continue;
        //		if (action == 4)
        //		{
        //			//RemoveItem(time);
        //			continue;
        //		}
        //		if ((action != 0) && (action != 0xE))
        //		{
        //			ParseReceivedData(action, buf);
        //		}
        //	}
        //}

        private T[] ParseReceivedData<T>(int action, InBuf buf, out long[] tickets) where T : FromBufReader, new()
        {
            if (action == 0)
            {
                int num = buf.Int();
                tickets = new long[num];
                for (int i = 0; i < num; i++)
                    tickets[i] = buf.Long();
                if (Api.Connection.TradeBuild <= 1892)
                    throw new NotImplementedException("TradeBuild <= 1892");
                var res = buf.Array<T>();
                buf.Bytes(16);
                return res;
            }
            else
            {
                Msg status = (Msg)buf.Int();
                var res = buf.Array<T>();
                tickets = new long[0];
                return res;
            }
        }



        private DealInternal LoadDeal(InBuf buf)
        {
            if (Api.Connection.TradeBuild <= 1892)
                throw new NotImplementedException("TradeBuild <= 1892");
            return buf.Struct<DealInternal>();
        }
    }
}
