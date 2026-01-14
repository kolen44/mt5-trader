using mtapi.mt5.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    public partial class MT5API
    {
        public void UpdateProfits(Quote quote = null)
        {
            UpdateProfitsTask(quote).Wait();
            CalcMarginAsync(true).Wait();
        }

        public async Task UpdateProfitsAsync(Quote quote = null)
        {
            await UpdateProfitsTask(quote);
            await CalcMarginAsync(true);
        }

        internal async Task UpdateProfitsTask(Quote quote = null)
        {
            CalcProfitAndPropsRunning = true;
            try
            {
                await UpdateOrderProfits(quote);
                UpdateAccountProfit();
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            finally
            {
                CalcProfitAndPropsRunning = false;
            }
        }

        //double LastProfit = 0;
        //DateTime LastProfitTime = new DateTime();

        public void UpdateAccountProfit()
        {
            double sum = 0;
            var orders = GetOpenedOrders();
            foreach (var item in orders)
                if (item.DealInternalIn != null)
                {
                    try
                    {
                        if (Symbols.Exist(item.Symbol) && item.Commission > 0 && (Symbols.Infos?.Count ?? 0) > 0)
                        {
                            item.Commission = CommissionOrZero(item.Commission, item.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                            if (item.DealInternalIn != null)
                                item.DealInternalIn.Commission = CommissionOrZero(item.DealInternalIn.Commission, item.DealInternalIn.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                        }
                    }catch {}
                    sum += item.Profit + item.Commission + item.Swap;
                }
            _AccountProfit = sum;
        }

        static bool NearlyEqual(double a, double b, double tol) => Math.Abs(a - b) <= tol;

        static double CommissionOrZero(double commission, double openPrice, int symbolDigits, double tickSize)
        {
            // tolerance = 10 ticks 
            var tol = tickSize > 0 ? 1000 * tickSize : 1000 * Math.Pow(10, -Math.Max(symbolDigits, 0));
            // if commission equals price 
            if (NearlyEqual(commission, openPrice, tol))
                return 0;
            // tiny commission close to zero → zero it
            if (Math.Abs(commission) < 1e-8) 
                return 0;
            return commission;
        }

        public double UpdateAccountProfit(List<Order> orders)
        {
	        double sum = 0;
            foreach (var item in orders)
                if (item.DealInternalIn != null)
                {
                    try
                    {
                        if (Symbols.Exist(item.Symbol) && item.Commission > 0 && (Symbols.Infos?.Count ?? 0) > 0)
                        {
                            item.Commission = CommissionOrZero(item.Commission, item.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                            if (item.DealInternalIn != null)
                                item.DealInternalIn.Commission = CommissionOrZero(item.DealInternalIn.Commission, item.DealInternalIn.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                        }
                    }catch {}
                    sum += item.Profit + item.Commission + item.Swap;
                }
	        _AccountProfit = sum;
	        return sum;
        }

		private async Task UpdateOrderProfits(Quote quote)
        {
            foreach (var item in GetOpenedOrders())
            {
                if (quote != null)
                {
                    if (item.OrderType == OrderType.Buy || item.OrderType == OrderType.Sell)
                    if (!Subscriber.Subscribed(item.Symbol))
                        _ = Subscriber.Subscribe(item.Symbol);
                    var info = Symbols.GetInfo(item.Symbol);
                
                    if (quote.Symbol.Contains(info.ProfitCurrency) && quote.Symbol.Contains(Symbols.Base.Currency))
                        ; // update profit
                    else if (quote.Symbol != item.Symbol)
                        continue; // skip
                }
                if (!Symbols.Exist(item.Symbol))
                    continue;
                if (item.OrderType == OrderType.Buy || item.OrderType == OrderType.Sell)
                {
                    var q = await GetQuoteInternal(item.Symbol, GetQuoteTimeoutMs);
#if DELAYED_SYMBOLS
                    lock (DelayedSymbols)
                        if (DelayedSymbols.ContainsKey(item.Symbol))
                            quote = DelayedSymbols[item.Symbol];
                            
#endif
                    if (q == null)
                        continue;
                    await OrderProfit.Update(item, q.Bid, q.Ask);
                }
            }
        }

        DateTime LastCalcMargin;

        //[MethodImpl(MethodImplOptions.Synchronized)]
        internal async Task CalcMarginAsync(bool force)
        {
            if(!force)
                if (DateTime.Now.Subtract(LastCalcMargin).TotalMilliseconds < 500)
                    return;
            LastCalcMargin = DateTime.Now;
            _AccountMargin = await CalcMargin(GetOpenedOrders());
        }

        private async Task<double> CalcMargin(Order[] orders)
        {
			Dictionary<string, SymbolMargin> syms = new Dictionary<string, SymbolMargin>();
			foreach (var order in orders)
			{
				if (!Symbols.Exist(order.Symbol))
					continue;
				if (!syms.ContainsKey(order.Symbol))
					syms.Add(order.Symbol, new SymbolMargin(this, order.Symbol));
				if (order.DealInternalIn != null)
				{
					var deal = new DealInternal();
					deal.Volume = order.Volume; // in case of partial closed order
					deal.Price = order.DealInternalIn.Price;
					deal.OpenPrice = order.DealInternalIn.OpenPrice;
					deal.VolumeRate = order.DealInternalIn.VolumeRate;
					deal.Type = order.DealInternalIn.Type;
					syms[order.Symbol].AcceptDeal(deal);
				}
				else if (order.OrderInternal != null)
					await syms[order.Symbol].AcceptOrder(order.OrderInternal, false);
			}
			double sum = 0;
			foreach (var item in syms.Values)
				sum += await item.GetTradeMargin();
            return sum;
		}

		/// <summary>
		/// Required margin
		/// </summary>
		/// <param name="id">Token returned by 'Connect' method</param>
		/// <param name="symbol">Symbol</param>
		/// <param name="lots">Lots</param>
		/// <param name="type">Buy or Sell</param>
		/// <param name="price">Price</param>
		public async Task<double> RequiredMargin(string symbol, double lots, DealType type = DealType.DealBuy, double price = 0)
		{
			if (price == 0)
                if (type == DealType.DealBuy)
					price = (await GetQuoteAsync(symbol, GetQuoteTimeoutMs)).Ask;
				else
					price = (await GetQuoteAsync(symbol, GetQuoteTimeoutMs)).Bid;
			Dictionary<string, SymbolMargin> syms = new Dictionary<string, SymbolMargin>();
            var order = new Order { Symbol = symbol, Lots = lots, DealType = type };
            double volumeRate = price;
			var margin_currency = Symbols.GetInfo(symbol).MarginCurrency;
			if (symbol.StartsWith(AccountCurrency))
                volumeRate = 1;
            if(Symbols.GetInfo(symbol).CalcMode == CalculationMode.CFDLeverage)
				volumeRate = 1;
			else if (!symbol.Contains(AccountCurrency) && AccountCurrency == margin_currency)
				volumeRate = 1;
			else if (!symbol.Contains(AccountCurrency))
            {
                var sym = AccountCurrency + margin_currency;
                var sym_reverse = margin_currency + AccountCurrency;
                bool found = false;
                foreach (var item in Symbols.Names)
                    if (item.Contains(sym))
                    {
                        try
                        {
                            volumeRate = 1 / (await GetQuoteAsync(sym, GetQuoteTimeoutMs)).Bid;
                            found = true;
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else if (item.Contains(sym_reverse))
                    {
                        try
                        {
                            volumeRate = (await GetQuoteAsync(sym_reverse, GetQuoteTimeoutMs)).Bid;
                            found = true;
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                if (!found) 
                    throw new Exception($"Cannot calculate required margin: {sym} and {sym_reverse} not found");
			}
			order.DealInternalIn = new DealInternal() { Symbol = symbol, Volume = (ulong)(lots * 100000000) , Price = price, OpenPrice = price,
            VolumeRate = volumeRate, Type = type};
            var orders = GetOpenedOrders();
			var before  = await CalcMargin(orders);
            Array.Resize(ref orders, orders.Length + 1);
            orders[orders.Length - 1] = order;
			var after = await CalcMargin(orders);
            return after - before;
		}

        /// <summary>
        /// Calculate order profit
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="openPrice">Open price</param>
        /// <param name="closePrice">Close price</param>
        /// <param name="lots">Lots</param>
        /// <param name="buy">If true - buy order, otherwise - sell order</param>
        /// <returns></returns>
        public double CalculateOrderProfit(string symbol, double openPrice, double closePrice, double lots, bool buy)
        {
            var order = new Order { Symbol = symbol, OpenPrice = openPrice, ClosePrice = closePrice, Lots = lots };
            if (buy)
                order.OrderType = OrderType.Buy;
            else
                order.OrderType = OrderType.Sell;
            order.DealInternalIn = new DealInternal() { Symbol = symbol, Price = openPrice, OpenPrice = openPrice, 
                Type = buy ? DealType.DealBuy : DealType.DealSell, Volume = (ulong)Math.Round(lots * 100000000, 0)
            }; 
            OrderProfit.Update(order, closePrice, closePrice).Wait();
            return order.Profit;
        }
    }
}
