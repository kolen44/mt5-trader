using mtapi.mt5.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
	class OrderProfit
	{
		MT5API QC;
        private static readonly SemaphoreSlim UpdateSemaphore = new SemaphoreSlim(1, 1);


        internal OrderProfit(MT5API qc)
		{
			QC = qc;
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal async Task Update(Order order, double bid, double ask)
		{
			if (!await UpdateSemaphore.WaitAsync(30000))
				throw new TimeoutException("OrderProfit.UpdateSemaphore timeout");
			try
			{
				if (order.DealInternalIn == null)
					return;
				if (!QC.Symbols.Exist(order.Symbol))
					return;
				var sym = QC.Symbols.GetInfo(order.Symbol);
				if (order.OrderType == OrderType.Buy)
					order.ClosePrice = bid;
				else if (order.OrderType == OrderType.Sell)
					order.ClosePrice = ask;
				if (!await UpdateSymbolTick(sym, QC.GetQuoteTimeoutMs))
					return;
				if (sym.CalcMode == CalculationMode.Forex)
				{
					if (order.OrderType == OrderType.Buy)
					{
						double lots = order.Lots * sym.bid_tickvalue;
						order.Profit = Math.Round(bid * lots, sym.Digits) - Math.Round(order.OpenPrice * lots, sym.Digits);
					}
					else if (order.OrderType == OrderType.Sell)
					{
						order.ClosePrice = ask;
						double lots = order.Lots * sym.ask_tickvalue;
						order.Profit = Math.Round(order.OpenPrice * lots, sym.Digits) - Math.Round(ask * lots, sym.Digits);
					}
				}

				else if (sym.CalcMode == CalculationMode.ExchangeStocks || sym.CalcMode == CalculationMode.ExchangeBounds
					|| sym.CalcMode == CalculationMode.ExchangeFutures || sym.CalcMode == CalculationMode.FORTSFutures ||
					sym.CalcMode == CalculationMode.ExchangeBounds)
				{
					var tick_size = QC.GetTickSize(order.Symbol);
					if (tick_size == 0)
						tick_size = 1;
					if (order.OrderType == OrderType.Buy)
						order.Profit = order.Lots * ((await QC.GetQuoteInternal(order.Symbol, QC.GetQuoteTimeoutMs)).Last - order.OpenPrice) / tick_size;
					else if (order.OrderType == OrderType.Sell)
						order.Profit = order.Lots * (order.OpenPrice - (await QC.GetQuoteInternal(order.Symbol, QC.GetQuoteTimeoutMs)).Last) / tick_size;
				}
				else if (sym.CalcMode == CalculationMode.CFDLeverage)
				{
					double price = 0;
					if (order.OrderType == OrderType.Buy)
						price = bid - order.OpenPrice;
					else if (order.OrderType == OrderType.Sell)
						price = order.OpenPrice - ask;
					var profit = price * order.Lots;
					var rate = (profit > 0) ? sym.bid_tickvalue : sym.ask_tickvalue;
					order.Profit = profit * rate;
				}
				else
				{
					double price = 0;
					if (order.OrderType == OrderType.Buy)
						price = (bid - order.OpenPrice) * sym.bid_tickvalue;
					else if (order.OrderType == OrderType.Sell)
						price = (order.OpenPrice - ask) * sym.ask_tickvalue;
					order.Profit = price * order.Lots;
					if (sym.CalcMode == CalculationMode.Futures)
						order.Profit /= QC.GetTickSize(order.Symbol);
				}
				order.Profit = Math.Round(order.Profit, sym.Digits);
			}
			finally
			{
                UpdateSemaphore.Release();
			}
		}

		async Task<int> GetTickRate(string symbol, Quote rate, int msTimeout)
		{
			if (!QC.Symbols.Exist(symbol))
				return 0;
			if (await QC.GetQuoteInternal(symbol, msTimeout) == null)
				return -1;
			var q = await QC.GetQuoteInternal(symbol, 0);
			rate.Bid = q.Bid;
			rate.Ask = q.Ask;
			return 1;
		}

		bool memcmp(string str1, int ind, string str2, int count)
		{
			return memcmp(str1.Substring(3), str2, count);
		}

		bool memcmp(string str1, string str2, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (i == str1.Length)
					if (str1.Length == str2.Length)
						return true;
					else
						return false;
				if (i == str2.Length)
					if (str1.Length == str2.Length)
						return true;
					else
						return false;
				if (str1[i] != str2[i])
					return false;
			}
			return true;
		}

		void memcpy(ref string dst, int dstind, string src, int srcind, int count)
		{
			string res = dst.Substring(0, dstind);
			for (int i = 0; i < count; i++)
				if (i + srcind >= src.Length)
					break;
				else
					res += src[i + srcind];
			for (int i = dstind + count; i < dst.Length; i++)
				res += dst[i];
			dst = res;
		}

		void memcpy(ref string dst, string src, int count)
		{
			string res = "";
			for (int i = 0; i < count; i++)
				if (i == src.Length)
					break;
				else 
					res += src[i];
			for (int i = count; i < dst.Length; i++)
				res += dst[i];
			dst = res;
		}

		internal async Task<bool> UpdateSymbolTick(SymbolInfo sym, int msTimeout)
		{
            var accountCurrency = QC.Symbols.Base.Currency;
			if (accountCurrency == "USC")
				accountCurrency = "USD";
            if (accountCurrency == "EUC")
                accountCurrency = "EUR";

            if (sym.CalcMode == CalculationMode.Forex || sym.CalcMode == CalculationMode.CalcMode5)
			{
                Quote rate = new Quote();
                string sym_symbol = sym.Currency;
                string cur1 = sym_symbol;
                string cur2 = "";

                if (memcmp(cur1, accountCurrency, 3))
				{

					var quote = await QC.GetQuoteInternal(sym_symbol, msTimeout);
					if (quote != null)
					{
						sym.bid_tickvalue = sym.ContractSize / quote.Bid;
						sym.ask_tickvalue = sym.ContractSize / quote.Ask;
					}
				}
				else if (memcmp(cur1, 3, accountCurrency, 3))
				{
					sym.bid_tickvalue = sym.ContractSize;
					sym.ask_tickvalue = sym.ContractSize;
				}
				else
				{
					memcpy(ref cur2, accountCurrency, 3);
					memcpy(ref cur2, 3, cur1, 3, 3);
					memcpy(ref cur2, 6, sym_symbol, 6, 6);
					int res;
					if (QC.Symbols.Exist(cur2))
					{
						var gr = QC.Symbols.GetGroup(cur2);
						if (gr.TradeMode == TradeMode.Disabled || gr.TradeMode == TradeMode.CloseOnly)
						{
							string cur3 = cur2;
							memcpy(ref cur3, 0, cur1, 3, 3);
							memcpy(ref cur3, 3, accountCurrency, 0, 3);
							var symbol = QC.Symbols.ExistStartsWith(cur3);
							if (symbol != null)
							{
								gr = QC.Symbols.GetGroup(symbol);
								if (gr.TradeMode != TradeMode.Disabled || gr.TradeMode != TradeMode.CloseOnly)
								{
									res = await GetTickRate(symbol, rate, msTimeout);
									if (res == 1)
									{
										sym.bid_tickvalue = sym.ContractSize * rate.Bid;
										sym.ask_tickvalue = sym.ContractSize * rate.Ask;
										return true;
									}
								}
							}
						}
					}
					res = await GetTickRate(cur2, rate, msTimeout);
					if (res == 1)
					{
						sym.bid_tickvalue = sym.ContractSize / rate.Bid;
						sym.ask_tickvalue = sym.ContractSize / rate.Ask;
					}
					else
					{
						memcpy(ref cur2, 0, cur1, 3, 3);
						memcpy(ref cur2, 3, accountCurrency, 0, 3);
						res = await GetTickRate(cur2, rate, msTimeout);
						if (res == 1)
						{
							sym.bid_tickvalue = sym.ContractSize * rate.Bid;
							sym.ask_tickvalue = sym.ContractSize * rate.Ask;
						}
						else
						{
							memcpy(ref cur2, "USD", 3);
							memcpy(ref cur2, 3, cur1, 3, 3);
							res = await GetTickRate(cur2, rate, msTimeout);
							if (res == 1)
							{
								sym.bid_tickvalue = sym.ContractSize / rate.Bid;
								sym.ask_tickvalue = sym.ContractSize / rate.Ask;
							}
							else
							{
								memcpy(ref cur2, 0, cur1, 3, 3);
								memcpy(ref cur2, 3, "USD", 0, 3);
								res = await GetTickRate(cur2, rate, msTimeout);
								if (res == 1)
								{
									sym.bid_tickvalue = sym.ContractSize * rate.Bid;
									sym.ask_tickvalue = sym.ContractSize * rate.Ask;
								}
							}
							memcpy(ref cur2, "USD", 3);
							memcpy(ref cur2, 3, accountCurrency, 0, 3);
							res = await GetTickRate(cur2, rate, msTimeout);
							if (res == 1)
							{
								sym.bid_tickvalue *= rate.Bid;
								sym.ask_tickvalue *= rate.Ask;
							}
							else
							{
								memcpy(ref cur2, accountCurrency, 3);
								memcpy(ref cur2, 3, "USD", 0, 3);
								res = await GetTickRate(cur2, rate, msTimeout);
								if (res == 1)
								{
									sym.bid_tickvalue /= rate.Bid;
									sym.ask_tickvalue /= rate.Ask;
								}
							}
						}
					}
				}
                if(QC.Symbols.Base.Currency == "USC" || QC.Symbols.Base.Currency == "EUC")
				{
					sym.bid_tickvalue *= 100;
                    sym.ask_tickvalue *= 100;
                }
                return (sym.bid_tickvalue > 0) && (sym.bid_tickvalue < double.MaxValue) && (sym.ask_tickvalue > 0) && (sym.ask_tickvalue < double.MaxValue);
			}
            double dTick = (sym.CalcMode == CalculationMode.Futures) ? sym.TickValue : sym.ContractSize;
			sym.bid_tickvalue = dTick;
			sym.ask_tickvalue = dTick;
			if (sym.ProfitCurrency != accountCurrency)
			{
                Quote rate = new Quote();
                string sym_symbol = sym.Currency;
                string cur2 = "";
                memcpy(ref cur2, sym.ProfitCurrency, 3);
                memcpy(ref cur2, 3, accountCurrency, 0, 3);
                if (cur2.Length > 6)
                    cur2 = cur2.Substring(0, 6);
                var res = await GetTickRate(cur2, rate, msTimeout);
                if (res == -1)
                    return false;
                if (res == 1)
                {
                    sym.bid_tickvalue *= rate.Bid;
                    sym.ask_tickvalue = sym.bid_tickvalue;
                    return true;
                }
                memcpy(ref cur2, accountCurrency, 3);
				memcpy(ref cur2, 3, sym.ProfitCurrency, 0, 3);
				if (cur2.Length > 6)
					cur2 = cur2.Substring(0, 6);
				res = await GetTickRate(cur2, rate, msTimeout);
				if (res == -1)
					return false;
				if (res == 1)
				{
					sym.bid_tickvalue /= rate.Bid;
					sym.ask_tickvalue = sym.bid_tickvalue;
					return true;
				}
                memcpy(ref cur2, 6, sym_symbol, 6, 6);
                res = await GetTickRate(cur2, rate, msTimeout);
                if (res == -1)
                    return false;
                if (res == 1)
                {
                    sym.bid_tickvalue /= rate.Bid;
                    sym.ask_tickvalue = sym.bid_tickvalue;
                    return true;
                }
                memcpy(ref cur2, sym.ProfitCurrency, 3);
				memcpy(ref cur2, 3, accountCurrency, 0, 3);
				res = await GetTickRate(cur2, rate, msTimeout);
				if (res == -1)
					return false;
				if (res == 1)
				{
					sym.bid_tickvalue *= rate.Bid;
					sym.ask_tickvalue = sym.bid_tickvalue;
					return true;
				}
				memcpy(ref cur2, "USD", 3);
				memcpy(ref cur2, 3, sym.ProfitCurrency, 0, 3);
				res = await GetTickRate(cur2, rate, msTimeout);
				if (res == -1)
					return false;
				if (res == 1)
				{
					sym.bid_tickvalue /= rate.Bid;
					sym.ask_tickvalue = sym.bid_tickvalue;
				}
				else
				{
					memcpy(ref cur2, sym.ProfitCurrency, 3);
					memcpy(ref cur2, 3, "USD", 0, 3);
					res = await GetTickRate(cur2, rate, msTimeout);
					if (res == -1)
						return false;
					if (res == 1)
					{
						sym.bid_tickvalue *= rate.Bid;
						sym.ask_tickvalue *= rate.Ask;
						//->94111D
					}
					else
					{
						//sym.bid_tickvalue = 0;
						//sym.ask_tickvalue = 0;
						return false;
					}
				}
				memcpy(ref cur2, "USD", 3);
				memcpy(ref cur2, 3, accountCurrency, 0, 3);
				res = await GetTickRate(cur2, rate, msTimeout);
				if (res == -1)
					return false;
				if (res == 1)
				{
					sym.bid_tickvalue *= rate.Bid;
					sym.ask_tickvalue *= rate.Ask;
				}
				else
				{
					memcpy(ref cur2, accountCurrency, 3);
					memcpy(ref cur2, 3, "USD", 0, 3);
					res = await GetTickRate(cur2, rate, msTimeout);
					if (res == -1)
						return false;
					if (res == 1)
					{
						sym.bid_tickvalue /= rate.Bid;
						sym.ask_tickvalue /= rate.Ask;
					}
					else
					{
						//sym.bid_tickvalue = 0;
						//sym.ask_tickvalue = 0;
						return false;
					}
				}

			}
            if (QC.Symbols.Base.Currency == "USC" || QC.Symbols.Base.Currency == "EUC")
            {
                sym.bid_tickvalue *= 100;
                sym.ask_tickvalue *= 100;
            }
            return (sym.bid_tickvalue > 0) && (sym.bid_tickvalue < double.MaxValue) && (sym.ask_tickvalue > 0) && (sym.ask_tickvalue < double.MaxValue);
		}
	}
}
