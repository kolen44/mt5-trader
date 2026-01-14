using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mtapi.mt5
{
    internal class EquityHistory
    {
        public List<EquityPoint> CalculateEquityHistory(DateTime from, MT5API api, EquityTimeframe timeframe, bool excludeSameBars,
            Dictionary<string, Dictionary<DateTime, Bar>> quoteHistory, List<Order> orders)
        {
            if (orders.Count == 0)
                return new List<EquityPoint>();
            var balanceHistory = new List<BalancePoint>();
            var currentBalance = api.Account.Balance;
            foreach (var item in orders)
            {
                balanceHistory.Add(new BalancePoint() { Balance = currentBalance, Time = item.CloseTime });
                currentBalance -= item.Profit + item.Commission + item.Swap;
            }
            balanceHistory.Add(new BalancePoint() { Balance = currentBalance });

            List<EquityPoint> equity = new List<EquityPoint>();
            var time = api.ServerTime;
            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
            equity.Add(new EquityPoint() { Balance = Math.Round(api.Account.Balance, 8), Equity = Math.Round(api.AccountEquity, 8), Time = time });
            var dayStart = new DateTime(time.Year, time.Month, time.Day);
            if ((int)timeframe < (int)EquityTimeframe.D1)
            {
                while (time.Subtract(dayStart).TotalMinutes % (int)timeframe > 0)
                    time = time.AddMinutes(-1);
            }
            else
            {
                time = new DateTime(time.Year, time.Month, time.Day);
            }
            var balanceEnumerator = balanceHistory.GetEnumerator();
            if (!balanceEnumerator.MoveNext())
                throw new Exception("Balance history is empty");
            var balance = balanceEnumerator.Current.Balance;
            if (orders.Count > 0)
            {
                //var start = orderHistory.Orders.Aggregate((min, next) => next.OpenTime < min.OpenTime ? next : min).OpenTime;
                //if ((int)timeframe >= (int)EquityTimeframe.D1)
                //    start = new DateTime(start.Year, start.Month, start.Day);
                var start = from;
                while (time >= start)
                {
                    if (balanceEnumerator.Current != null)
                        while (time < balanceEnumerator.Current.Time)
                            if (!balanceEnumerator.MoveNext())
                                break;
                    if (balanceEnumerator.Current != null)
                        balance = balanceEnumerator.Current.Balance;
                    var closedProfit = GetProfit(orders, quoteHistory, time, api);
                    var openedProfit = GetProfit(api.GetOpenedOrders().ToList(), quoteHistory, time, api);
                    var currentEquity = balance + closedProfit + openedProfit;
                    bool skip = false;
                    if (timeframe == EquityTimeframe.D1)
                        if (equity[0].Time.Year == time.Year && equity[0].Time.Month == time.Month && equity[0].Time.Day == time.Day)
                            skip = true;
                    var current = new EquityPoint() { Balance = Math.Round(balance, 8), Equity = Math.Round(currentEquity, 8), Time = time };
                    if (!skip)
                        if (excludeSameBars)
                        {
                            var previuos = equity.Last();
                            if (Math.Round(current.Balance, 8) != Math.Round(previuos.Balance, 8) && Math.Round(current.Equity, 8) != Math.Round(previuos.Equity, 8))
                            {
                                if (previuos.Time.Subtract(current.Time).TotalMinutes > (int)timeframe)
                                    equity.Add(new EquityPoint() { Balance = previuos.Balance, Equity = previuos.Equity, Time = current.Time.AddMinutes((int)timeframe) });
                                equity.Add(current);
                            }
                        }
                        else
                            equity.Add(current);
                    time = time.AddMinutes(-(int)timeframe);
                }
            }
            else
                equity.Add(new EquityPoint() { Balance = Math.Round(api.Account.Balance, 8), Equity = Math.Round(api.AccountEquity, 8), Time = from });
            equity.Reverse();
            if (equity.Count > 0)
            {
                double startBalance = equity[0].Balance;
                foreach (var item in equity)
                {
                    item.RealizedPL = item.Balance - startBalance;
                    item.UnrealizedPL = item.Equity - item.Balance;
                }
            }
            if (equity.Count > 0) // remove zero bars at the beginning
            {
                var res = new List<EquityPoint>();
                int i = 0;
                while (equity[i].Balance == 0 && equity[i].Equity == 0 && i < equity.Count)
                    i++;
                for (; i < equity.Count; i++)
                    res.Add(equity[i]);
                if (res.Count == 0)
                {
                    res.Add(equity.First());
                    if (equity.Count > 1)
                        res.Add(equity.Last());
                }
                return res;
            }
            else
                return equity;
        }

        public List<EquityPoint> CalculateEquityHistory(DateTime from, MT5API api, EquityTimeframe timeframe, bool excludeSameBars)
        {
            var orderHistory = api.DownloadOrderHistoryAsync(from, api.ServerTime, OrderSort.CloseTime, false).Result;
            var symbols = new HashSet<string>();
            foreach (var item in orderHistory.Orders)
                if (api.Symbols.Exist   (item.Symbol))
                    if (!symbols.Contains(item.Symbol))
                        symbols.Add(item.Symbol);
            foreach (var item in api.GetOpenedOrders())
                if (api.Symbols.Exist(item.Symbol))
                    if (!symbols.Contains(item.Symbol))
                        symbols.Add(item.Symbol);
            var minOpenTime = orderHistory.Orders.Min(item => item.OpenTime);
            var quoteHistory = new Dictionary<string, Dictionary<DateTime, Bar>>();
            foreach (var symbol in symbols)
            {
                var hist = api.DownloadQuoteHistoryAsync(symbol, minOpenTime, api.ServerTime, (int)timeframe).Result;
                var bars = new Dictionary<DateTime, Bar>();
                foreach (var item in hist)
                    bars.Add(item.Time, item);
                quoteHistory.Add(symbol, bars);
            }
            return CalculateEquityHistory(from, api, timeframe, excludeSameBars, quoteHistory, orderHistory.Orders);
        }

        public double GetProfit(List<Order> orders, Dictionary<string, Dictionary<DateTime, Bar>> quoteHistory, DateTime time, MT5API api)
        {
            double profit = 0;
            foreach (var item in orders)
            {
                if (item.CloseTime == default || time < item.CloseTime)
                    if (time > item.OpenTime)
                        if (item.OrderType == OrderType.Buy || item.OrderType == OrderType.Sell)
                        {
                            var quoteHist = quoteHistory[item.Symbol];
                            if (quoteHist.Count > 0)
                            {
                                var price = GetPrice(quoteHist, time);
                                var isBuy = item.OrderType == OrderType.Buy ? true : false;
                                profit += api.CalculateOrderProfit(item.Symbol, item.OpenPrice, price, item.Lots, isBuy);
                            }
                        }
            }
            return profit;
        }


        public double GetPrice(Dictionary<DateTime, Bar> bars, DateTime time)
        {
            if (bars.ContainsKey(time))
                return bars[time].ClosePrice;
            foreach (var item in bars.Values)
                if (item.Time >= time)
                    return item.ClosePrice;
            return bars.Values.Last().ClosePrice;
        }
    }

    /// <summary>
    /// Point of equity history
    /// </summary>
    public class EquityPoint
    {
        /// <summary>
        /// Time
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// Balance
        /// </summary>
        public double Balance;
        /// <summary>
        /// Equity
        /// </summary>
        public double Equity;
        /// <summary>
        /// Balance raw drawdown
        /// </summary>
        public double BalanceDrawdownRaw;
        /// <summary>
        /// Balance relative drawdown
        /// </summary>
        public double BalanceDrawdownRelative;
        /// <summary>
        /// Balance raw drawdown
        /// </summary>
        public double EquityDrawdownRaw;
        /// <summary>
        /// Balance relative drawdown
        /// </summary>
        public double EquityDrawdownRelative;
        /// <summary>
        /// Realized profit or loss
        /// </summary>
        public double RealizedPL;
        /// <summary>
        /// Unrealized profit or loss
        /// </summary>
        public double UnrealizedPL;

    }

    /// <summary>
    /// Point of balance history
    /// </summary>
    public class BalancePoint
    {
        /// <summary>
        /// Time
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// Balance
        /// </summary>
        public double Balance;
        /// <summary>
        /// Balance
        /// </summary>
        public double Profit;
    }

    /// <summary>
	/// Equity history timeframe 
	/// </summary>
	public enum EquityTimeframe
    {
        /// <summary>
        /// 1 hour. 
        /// </summary>
        H1 = 60,
        /// <summary>
        /// 4 hour.
        /// </summary>
        H4 = 240,
        /// <summary>
        /// Daily. 
        /// </summary>
        D1 = 1440,
    }
}
