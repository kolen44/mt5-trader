
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Pending, market or history order
    /// </summary>
    public class Order
    {
        public long Ticket;
        public double Profit;
        internal double CloseProfit;

        public double Swap;
        public double Commission;
        public double Fee;
        public double ClosePrice;
        public DateTime CloseTime;
        public double CloseLots;
        public ulong CloseVolume
        {
            get { return (ulong)Math.Round(CloseLots * 100000000, 0); }
        }
        public string CloseComment;

        public double OpenPrice;
        public DateTime OpenTime;
        public double Lots;
        public ulong Volume
        {
            get { return (ulong)Math.Round(Lots * 100000000, 0); }
        }
        public double ContractSize;
        public long ExpertId;
        public PlacedType PlacedType;
        public OrderType OrderType;
        public DealType DealType;
        public string Symbol;
        public string Comment;
        public OrderState State;
        public double StopLoss;
        public double TakeProfit;
        public int RequestId;
        public int Digits;
        public double ProfitRate;
        public double StopLimitPrice;
        public DealInternal DealInternalIn;
        public DealInternal DealInternalOut;
        public OrderInternal OrderInternal;
        internal MT5API Api;
        public DealInternal[] PartialCloseDeals = new DealInternal[0];
        public DealInternal[] PartialFillDeals = new DealInternal[0];

        public ExpirationType ExpirationType
		{
			get
			{
                return OrderInternal == null ? ExpirationType.GTC : OrderInternal.ExpirationType;
            }
		}

        public DateTime ExpirationTime
        {
            get
            {
				return OrderInternal == null ? new DateTime() : ConvertTo.DateTime(OrderInternal.ExpirationTime);

            }
        }

        public FillPolicy FillPolicy
        {
            get
            {
                return OrderInternal == null ? FillPolicy.FillOrKill : OrderInternal.FillPolicy;

            }
        }

        /// <summary>
        /// Open timestamp in milliseconds
        /// </summary>
        public long OpenTimestampUTC
        {
            get
            {
                return ConvertTo.LongMs(OpenTime) - Api.ServerTimeZoneInMinutes * 60000;
            }
        }

        /// <summary>
        /// Close timestamp in milliseconds
        /// </summary>
        public long CloseTimestampUTC
        {
            get
            {
                return ConvertTo.LongMs(CloseTime) - Api.ServerTimeZoneInMinutes * 60000;
            }
        }

        public Order()
		{

		}

        public Order Clone()
		{
            var res = new Order();
            res.Update(this);
            return res;
		}



        internal void UpdateNetting(Order update, long ticket)
        {
            Api = update.Api;
            if (OrderType == OrderType.Sell)
                Lots = -Lots;
            if (update.OrderType == OrderType.Buy)
                Lots += update.Lots;
            else
                Lots -= update.Lots;
            if (Lots < 0)
            {
                Lots = -Lots;
                OrderType = OrderType.Sell;
                DealType = DealType.DealSell;
            }
            else
            {
                OrderType = OrderType.Buy;
                DealType = DealType.DealBuy;
            }
            Lots = Math.Round(Lots, 8);
            Ticket = ticket;
            StopLimitPrice = update.StopLimitPrice;
            Profit = update.Profit;
            Swap = update.Swap;
            Commission = update.Commission;
            Fee = update.Fee;
            ClosePrice = update.ClosePrice;
            CloseTime = update.CloseTime;
            CloseLots = update.CloseLots;
            OpenPrice = update.OpenPrice;
            OpenTime = update.OpenTime;
            ContractSize = update.ContractSize;
            if (update.ExpertId != 0)
                ExpertId = update.ExpertId;
            PlacedType = update.PlacedType;
            OrderType = update.OrderType;
            Symbol = update.Symbol;
            Comment = update.Comment;
            State = update.State;
            StopLoss = update.StopLoss;
            TakeProfit = update.TakeProfit;
            if (update.RequestId != 0)
                RequestId = update.RequestId;
            Digits = update.Digits;
            ProfitRate = update.ProfitRate;
            DealInternalIn = update.DealInternalIn;
            DealInternalOut = update.DealInternalOut;
            OrderInternal = update.OrderInternal;
        }

        internal bool Update(Order order)
        {
            bool changed = false;

            changed |= !Equals(Api, order.Api); Api = order.Api;
            changed |= Ticket != order.Ticket; Ticket = order.Ticket;
            changed |= StopLimitPrice != order.StopLimitPrice; StopLimitPrice = order.StopLimitPrice;
            changed |= Profit != order.Profit; Profit = order.Profit;
            changed |= Swap != order.Swap; Swap = order.Swap;
            changed |= Commission != order.Commission; Commission = order.Commission;
            changed |= Fee != order.Fee; Fee = order.Fee;
            if(order.ClosePrice != 0)
                ClosePrice = order.ClosePrice;
            changed |= CloseTime != order.CloseTime; CloseTime = order.CloseTime;
            changed |= CloseLots != order.CloseLots; CloseLots = order.CloseLots;
            changed |= OpenPrice != order.OpenPrice; OpenPrice = order.OpenPrice;
            changed |= OpenTime != order.OpenTime; OpenTime = order.OpenTime;
            changed |= ContractSize != order.ContractSize; ContractSize = order.ContractSize;

            if (order.ExpertId != 0)
            {
                changed |= ExpertId != order.ExpertId;
                ExpertId = order.ExpertId;
            }

            changed |= PlacedType != order.PlacedType; PlacedType = order.PlacedType;
            changed |= OrderType != order.OrderType; OrderType = order.OrderType;
            changed |= DealType != order.DealType; DealType = order.DealType;
            changed |= Symbol != order.Symbol; Symbol = order.Symbol;
            changed |= Comment != order.Comment; Comment = order.Comment;
            changed |= State != order.State; State = order.State;
            changed |= StopLoss != order.StopLoss; StopLoss = order.StopLoss;
            changed |= TakeProfit != order.TakeProfit; TakeProfit = order.TakeProfit;

            if (order.RequestId != 0)
            {
                changed |= RequestId != order.RequestId;
                RequestId = order.RequestId;
            }

            changed |= Digits != order.Digits; Digits = order.Digits;
            changed |= ProfitRate != order.ProfitRate; ProfitRate = order.ProfitRate;
            if (DealInternalIn == null)
            {
                if (order.DealInternalIn != null)
                    changed = true;
                DealInternalIn = order.DealInternalIn;
                changed |= Lots != order.Lots; Lots = order.Lots;
            }
            else
            {
                if (!DealInternalIn.AreEquals(order.DealInternalIn))
                {
                    changed = true;
                    var partialFillDeals = PartialFillDeals.ToList();
                    partialFillDeals.Add(order.DealInternalIn);
                    PartialFillDeals = partialFillDeals.ToArray();
                    //Lots += order.Lots;
                    Lots = order.Lots;
                }                
            }

            if (DealInternalOut == null)
            {
                if (order.DealInternalOut != null)
                    changed = true;
            }
            else
                changed |= !DealInternalOut.AreEquals(order.DealInternalOut); 
            DealInternalOut = order.DealInternalOut;

            if (OrderInternal == null)
            {
                if (order.OrderInternal != null)
                    changed = true;
            }
            else
                changed |= !OrderInternal.AreEquals(order.OrderInternal); 
            OrderInternal = order.OrderInternal;

            return changed;
        }

        internal void Update(DealInternal item)
        {
            if (item.Direction == Direction.In)
            {
                DealInternalIn = item;
                OpenTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                OpenPrice = item.OpenPrice;
                Lots = item.Lots;
                ContractSize = item.ContractSize;
                PlacedType = (PlacedType)item.PlacedType;
                Ticket = item.PositionTicket;
                DealType = item.Type;
                if (DealType == DealType.DealSell)
                    OrderType = OrderType.Sell;
                ExpertId = item.ExpertId;
                Symbol = item.Symbol;
                Commission += item.Commission;
                Swap += item.Swap;
                Fee += item.Fee;
                Comment = item.Comment;
                StopLoss = item.StopLoss;
                TakeProfit = item.TakeProfit;
                Digits = item.Digits;
                ProfitRate = item.ProfitRate;
            }
            else if (item.Direction == Direction.Out || item.Direction == Direction.OutBy)
            {
                DealInternalOut = item;
                CloseTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                ClosePrice = item.OpenPrice;
                CloseLots += item.Lots;
                CloseProfit += item.Profit;
                Commission += item.Commission;
                Swap += item.Swap;
                Fee += item.Fee;
                CloseComment = item.Comment;
                Digits = item.Digits;
                ProfitRate = item.ProfitRate;
            }
        }

        internal Order(DealInternal[] deals, int requestId, MT5API api)
        {
            Api = api;
            RequestId = requestId;
            foreach (var item in deals)
            {
                if (item.Direction == Direction.In || item.Direction == Direction.InOut)
                {
                    DealInternalIn = item;
                    OpenTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                    OpenPrice = item.OpenPrice;
                    Lots = item.Lots;
                    ContractSize = item.ContractSize;
                    PlacedType = (PlacedType)item.PlacedType;
                    if (item.Direction == Direction.InOut)
                        Ticket = item.OrderTicket;
                    else
                        Ticket = item.PositionTicket;
                    if (Ticket == 0)
                        Ticket = item.TicketNumber;
                    DealType = item.Type;
                    if (DealType == DealType.DealSell)
                        OrderType = OrderType.Sell;
                    ExpertId = item.ExpertId;
                    Symbol = item.Symbol;
                    Commission += item.Commission;
                    Swap += item.Swap;
                    Fee += item.Fee;
                    Comment = item.Comment;
                    StopLoss = item.StopLoss;
                    TakeProfit = item.TakeProfit;
                    Digits = item.Digits;
                    ProfitRate = item.ProfitRate;
                }
                else if (item.Direction == Direction.Out || item.Direction == Direction.OutBy)
                {
                    DealInternalOut = item;
                    CloseTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                    ClosePrice = item.OpenPrice;
                    CloseLots = item.Lots;
                    CloseProfit += item.Profit;
                    Commission += item.Commission;
                    //Swap += item.Swap;
                    Fee += item.Fee;
                    CloseComment = item.Comment;
                    Digits = item.Digits;
                    ProfitRate = item.ProfitRate;
                    Profit += item.Profit;
                    if (Ticket == 0)
                        Ticket = item.OrderTicket;
                }
            }
        }

        internal void UpdateOnStop(DealInternal item, bool unusual = false)
        {
            DealInternalOut = item;
            CloseTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
            if (unusual)
                ClosePrice = item.Price;
            else
                ClosePrice = item.OpenPrice;
            CloseLots = item.Lots;
            Commission = item.Commission;
            Swap = item.Swap;
            Fee = item.Fee;
            Profit = item.Profit;
            Digits = item.Digits;
            ProfitRate = item.ProfitRate;
        }

        internal Order(DealInternal[] deals, MT5API api)
        {
            Api = api;
            var sortedDeals = deals.OrderBy(d => d.OpenTime).ToArray();
            foreach (var item in sortedDeals)
            {
                if (item.Direction == Direction.In)
                {
                    if (DealInternalIn == null)
                    {
                        DealInternalIn = item;
                        OpenTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                        OpenPrice = item.OpenPrice;
                        Lots = item.Lots;
                        ContractSize = item.ContractSize;
                        PlacedType = (PlacedType)item.PlacedType;
                        if (item.PositionTicket != 0)
                            Ticket = item.PositionTicket;
                        DealType = item.Type;
                        if (DealType == DealType.DealSell)
                            OrderType = OrderType.Sell;
                        ExpertId = item.ExpertId;
                        Symbol = item.Symbol;
                        Commission += item.Commission;
                        Swap += item.Swap;
                        Fee += item.Fee;
                        StopLoss = item.StopLoss;
                        TakeProfit = item.TakeProfit;
                        Digits = item.Digits;
                        ProfitRate = item.ProfitRate;
                        Comment = item.Comment;
                    }
                    else
                    {
                        var partialFillDeals = PartialFillDeals.ToList();
                        partialFillDeals.Add(item);
                        PartialFillDeals = partialFillDeals.ToArray();
                        Lots += item.Lots;
                        if (Ticket == 0 && item.PositionTicket != 0)
                            Ticket = item.PositionTicket;
                        Commission += item.Commission;
                        Swap += item.Swap;
                        Fee += item.Fee;
                    }
                }
                if (item.Direction == Direction.Out || item.Direction == Direction.OutBy)
                {
                    DealInternalOut = item;
                    CloseTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
                    ClosePrice = (ClosePrice * CloseLots + item.OpenPrice * item.Lots) / (CloseLots + item.Lots);
                    CloseLots += item.Lots;
                    CloseComment = item.Comment;
                    Commission += item.Commission;
                    Swap += item.Swap;
                    Fee += item.Fee;
                    Profit += item.Profit;
                    Digits = item.Digits;
                    ProfitRate = item.ProfitRate;
                    StopLoss = item.StopLoss;
                    TakeProfit = item.TakeProfit;
                    if (item.PositionTicket != 0)
                        Ticket = item.PositionTicket;
                }
            }
            double totalLots = 0;
            double weightedOpenPriceSum = 0;

            foreach (var deal in deals)
            {
                if (deal.Direction == Direction.In)
                {
                    totalLots += deal.Lots;
                    weightedOpenPriceSum += deal.OpenPrice * deal.Lots;
                }
            }
            double summaryOpenPrice = totalLots > 0 ? weightedOpenPriceSum / totalLots : 0;
            OpenPrice = summaryOpenPrice;
            PartialCloseDeals = GetPartialCloseDeals(deals);
        }


        public DealInternal[] GetPartialCloseDeals(DealInternal[] deals)
        {
            var res = new List<DealInternal>();
            foreach (var item in deals)
                if(item.Direction != Direction.In)
                    if(item.Type != DealType.Balance)
                        res.Add(item);
            if (res.Count > 0 && Lots == CloseLots)
            {
                res.Sort((x, y) => x.OpenTime.CompareTo(y.OpenTime));
                res.RemoveAt(res.Count - 1);
            }
            return res.ToArray();
        }

        public static DealInternal[] SortAndTrimDealsNotClosed(DealInternal[] deals)
        {
            if (deals == null || deals.Length <= 1)
            {
                // Return an empty array if deals is null or has 2 or fewer elements
                return Array.Empty<DealInternal>();
            }

            // Sort the array by OpenTime in ascending order
            Array.Sort(deals, (x, y) => x.OpenTime.CompareTo(y.OpenTime));

            // Create a new array excluding the first and last elements
            return deals.Skip(1).Take(deals.Length - 1).ToArray();
        }

        internal Order(DealInternal item, MT5API api)
        {
            Api = api;
            DealInternalIn = item;
            OpenTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
            OpenPrice = item.OpenPrice;
            Lots = item.Lots;
            ContractSize = item.ContractSize;
            PlacedType = item.PlacedType;
            Ticket = item.PositionTicket;
            if (Ticket == 0)
                Ticket = item.TicketNumber;
            DealType = item.Type;
            if (DealType == DealType.DealBuy)
                OrderType = OrderType.Buy;
            if (DealType == DealType.DealSell)
                OrderType = OrderType.Sell;
            if (DealType == DealType.Balance)
            {
                OrderType = OrderType.Balance;
                CloseTime = OpenTime;
            }
			if (DealType == DealType.Credit)
			{
				OrderType = OrderType.Credit;
				CloseTime = OpenTime;
			}
			ExpertId = item.ExpertId;
            Symbol = item.Symbol;
            Commission = item.Commission;
            Swap = item.Swap;
            Fee = item.Fee;
            Comment = item.Comment;
            Profit = item.Profit;
            StopLoss = item.StopLoss;
            TakeProfit = item.TakeProfit;
            State = OrderState.Filled;
            Digits = item.Digits;
            ProfitRate = item.ProfitRate;
        }

        internal Order(OrderInternal item, MT5API api)
        {
            Api = api;
            OrderInternal = item;
            StopLimitPrice = item.StopLimitPrice;
            OpenTime = ConvertTo.DateTimeMs(item.OpenTimeMs);
            if (item.ExecutionTime != 0)
                CloseTime = item.ExecutionTimeAsDateTime;
            OpenPrice = item.OpenPrice;
            Lots = item.Lots;
            ContractSize = item.ContractSize;
            PlacedType = (PlacedType)item.PlacedType;
            Ticket = item.TicketNumber;
            OrderType = (OrderType)item.Type;
            ExpertId = item.ExpertId;
            Symbol = item.Symbol;
            Comment = item.Comment;
            State = (OrderState)item.State;
            StopLoss = item.StopLoss;
            TakeProfit = item.TakeProfit;
            Digits = item.Digits;
            ProfitRate = item.ProfitRate;
        }

        public Order(OrderProgress progr, DateTime serverTime, MT5API api) // deal executed - OrderSend market
        {
            Api = api;
            OpenTime = serverTime;
            OpenPrice = progr.TradeResult.OpenPrice;
            Lots = (double)progr.TradeResult.Volume / 100000000;
            PlacedType = (PlacedType)progr.TradeRequest.PlacedType;
            Ticket = progr.TradeResult.TicketNumber;
            OrderType = progr.TradeRequest.OrderType;
            if (progr.TradeRequest.OrderType.ToString().StartsWith("Buy"))
                DealType = DealType.DealBuy;
            if (progr.TradeRequest.OrderType.ToString().StartsWith("Sell"))
                DealType = DealType.DealSell;
            ExpertId = progr.TradeRequest.ExpertId;
            Symbol = progr.TradeRequest.Currency;
            Comment = progr.TradeResult.Comment;
            StopLoss = progr.TradeRequest.StopLoss;
            TakeProfit = progr.TradeRequest.TakeProfit;
            State = OrderState.Filled;
            RequestId = progr.TradeRequest.RequestId;
            Digits = progr.TradeRequest.Digits;
        }

        public Order(OrderProgress progr, DateTime serverTime, Order order, MT5API api) // deal executed - OrderClose market
        {
            Api = api;
            CloseTime = serverTime;
            ClosePrice = progr.TradeResult.OpenPrice;
            CloseLots = (double)progr.TradeResult.Volume / 100000000;
            //PlacedType = (PlacedType)progr.TradeRequest.PlacedType;
            Ticket = progr.TradeResult.TicketNumber;
            //OrderType = progr.TradeRequest.OrderType;
            //if (progr.TradeRequest.OrderType.ToString().StartsWith("Buy"))
            //    DealType = DealType.DealBuy;
            //if (progr.TradeRequest.OrderType.ToString().StartsWith("Sell"))
            //    DealType = DealType.DealSell;
            //ExpertId = progr.TradeRequest.ExpertId;
            Symbol = progr.TradeRequest.Currency;
            //Commission = 
            //Swap = item.Swap;
            Comment = progr.TradeResult.Comment;
            //StopLoss = progr.TradeRequest.StopLoss;
            //TakeProfit = progr.TradeRequest.TakeProfit;
            State = OrderState.Filled;
            RequestId = progr.TradeRequest.RequestId;
            Digits = progr.TradeRequest.Digits;
            if (order != null)
            {
                OrderInternal = order.OrderInternal;
                DealInternalIn = order.DealInternalIn;
                DealInternalOut = order.DealInternalOut;
                OrderType = order.OrderType;
                DealType = order.DealType;
                ExpertId = order.ExpertId;
                Commission = order.Commission;
                Swap = order.Swap;
                Fee = order.Fee;
                OpenTime = order.OpenTime;
                Lots = order.Lots;
                TakeProfit = order.TakeProfit;
                StopLoss = order.StopLoss;
                PlacedType = order.PlacedType;
                ProfitRate = order.ProfitRate;
            }
        }
	}
}
