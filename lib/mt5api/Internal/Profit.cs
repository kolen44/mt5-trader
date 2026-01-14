using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
	internal class Profit
	{
		MT5API Api;

		

		internal Profit(MT5API api)
		{
			Api = api;
		}
		public void Calculate(Quote quote)
		{
			foreach (var item in Api.GetOpenedOrders())
				if (item.OrderType == OrderType.Buy || item.OrderType == OrderType.Sell)
					if (item.Symbol == quote.Symbol)
						try
						{
							var closeprice = item.OrderType == OrderType.Buy ? quote.Ask : quote.Bid;
							var sym = Api.Symbols.GetInfo(item.Symbol);
							var deal = item.DealInternalIn;
							CalculateProfit(sym, deal.Type == DealType.DealBuy, deal.Lots, deal.ContractSize, deal.OpenPrice, closeprice, deal);
						}
						catch (Exception ex)
						{
							Api.Log.exception(ex, Api);
						}
					else if (!Api.IsSubscribed(item.Symbol))
						Api.Subscribe(item.Symbol);
		}


		double AsSize(double value, double lots)
		{
			return Math.Round(lots * value, 8);
		}

		double AsLots(double value)
		{
			return Math.Round((double)value * 1.0e-8, 8);
		}

		internal async Task CalculateProfit(SymbolInfo sym, bool buy, double volume, double lots, double openPrice, double price, DealInternal deal)
		{
			double profit;
			switch (sym.CalcMode)
			{
				case  CalculationMode.Forex:
				case  CalculationMode.CalcMode5:
					profit = AsSize(volume, lots);
					if (buy)
						profit = Math.Round(profit * price, sym.Precision) - Math.Round(profit * openPrice, sym.Precision);
					else
						profit = Math.Round(profit * openPrice, sym.Precision) - Math.Round(profit * price, sym.Precision);
					break;
				case CalculationMode.Futures:
				case CalculationMode.ExchangeFutures:
				case CalculationMode.ExchangeOption:
				case CalculationMode.ExchangeMarginOption:
					profit = AsLots(volume);
					if (buy)
						profit *= price - openPrice;
					else
						profit *= openPrice - price;
					profit *= sym.TickValue;
					if (sym.TickSize > 0)
						profit /= sym.TickSize;
					break;
				case CalculationMode.FORTSFutures:
					{
						profit = AsLots(volume);
						double v1 = sym.TickValue * openPrice;
						double v2 = sym.TickValue * price;
						if (sym.TickSize > 0)
						{
							v1 /= sym.TickSize;
							v2 /= sym.TickSize;
						}
						v1 = Math.Round(v1, sym.Precision);
						v2 = Math.Round(v2, sym.Precision);
						if (buy)
							profit *= v2 - v1;
						else
							profit *= v1 - v2;
						break;
					}
				case CalculationMode.ExchangeBounds:
					profit = AsSize(volume, lots);
					if (buy)
						profit = Math.Round(profit * price * sym.FaceValue / 100.0, sym.Precision) -
							Math.Round(profit * openPrice * sym.FaceValue / 100.0, sym.Precision);
					else
						profit = Math.Round(profit * openPrice * sym.FaceValue / 100.0, sym.Precision) -
							Math.Round(profit * price * sym.FaceValue / 100.0, sym.Precision);
					break;
				case CalculationMode.Collateral:
					profit = 0;
					break;
				default:
					profit = AsSize(volume, lots);
					if (buy)
						profit *= price - openPrice;
					else
						profit *= openPrice - price;
					break;
			}
			if ((profit > 1.0e11) || (profit < -1.0e11))
				profit = 0;
			//if ((price < 0) || ((sym.CalcMode != CalculationMode.Collateral) && price > 0))
			//	profit = 0;
			BidAskWrapper bidAndAsk = new BidAskWrapper();
			await GetAskBidProfitRate(sym.ProfitCurrency, Api.Symbols.Base.Currency, bidAndAsk, sym, price);
			double rate;
			if (((sym.CalcMode == CalculationMode.Forex) || (sym.CalcMode ==  CalculationMode.CalcMode5)) && (sym.s550 & 1)==0)
				rate = buy ? bidAndAsk.Ask : bidAndAsk.Bid;
			else
				rate = (profit > 0) ? bidAndAsk.Bid : bidAndAsk.Ask;
			profit /= rate;
			deal.ProfitRate = rate;
			deal.Profit = profit;
			//return profit;
			//return Math.Round(profit, sym.Precision);
		}



		async Task GetAskBidProfitRate(string cur1, string cur2, BidAskWrapper bidAndAsk, SymbolInfo sym, double price)
		{
			if (await GetBidAskRate(cur1, cur2, bidAndAsk, sym, price))
				return;
			BidAskWrapper toUSD = new BidAskWrapper();
            BidAskWrapper fromUSD = new BidAskWrapper();
			if (!await GetBidAskRate(cur1, "USD", toUSD, sym, price) || !await GetBidAskRate("USD", cur2, fromUSD, sym, price))
			{
                bidAndAsk.Bid = 0;
                bidAndAsk.Ask = 0;
				return;
			}
            bidAndAsk.Bid = toUSD.Ask * fromUSD.Ask;
            bidAndAsk.Ask = toUSD.Bid * fromUSD.Bid;
		}

		class BidAskWrapper
		{
			public double Bid = 0;
			public double Ask = 0;
		}
        async Task<bool> GetBidAskRate(string cur1, string cur2, BidAskWrapper bidAskWrapper, SymbolInfo sym, double price)
		{
			string cur = Api.Symbols.ExistStartsWith(cur1 + cur2);
			if (cur != null)
			{
				if (price != 0 && ((sym.CalcMode == CalculationMode.Forex) || (sym.CalcMode == CalculationMode.CalcMode5)) &&
					((sym.s550 & 1) == 0) && sym.Currency == cur)
				{
                    bidAskWrapper.Bid = price;
                    bidAskWrapper.Ask = price;
					return true;
				}
				Api.Subscribe(cur);
				if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
				{
                    bidAskWrapper.Bid = 0;
                    bidAskWrapper.Ask = 0;
					return false;
				}
                bidAskWrapper.Bid = (await Api.GetQuoteInternal(cur, 0)).Bid;
                bidAskWrapper.Ask = (await Api.GetQuoteInternal(cur, 0)).Ask;
				return true;
			}
			cur = Api.Symbols.ExistStartsWith(cur2+ cur1);
			if (cur != null)
			{
				if (price != 0 && ((sym.CalcMode == CalculationMode.Forex) || (sym.CalcMode == CalculationMode.CalcMode5)) &&
					((sym.s550 & 1) == 0) && sym.Currency == cur)
				{
                    bidAskWrapper.Bid = 1.0 / price;
                    bidAskWrapper.Ask = 1.0 / price;
					return true;
				}
				Api.Subscribe(cur);
				if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
				{
                    bidAskWrapper.Bid = 0;
                    bidAskWrapper.Ask = 0;
					return false;
				}
                bidAskWrapper.Bid = (await Api.GetQuoteInternal(cur, 0)).Bid;
                bidAskWrapper.Ask = (await Api.GetQuoteInternal(cur, 0)).Ask;
				return true;
			}
            bidAskWrapper.Bid = 0;
            bidAskWrapper.Ask = 0;
			return false;
		}

	}
}
/*
 		double CalcProfit(SymBaseInfo symBase, SymbolInfo sym, OrderType type,
					double openPrice, double price, double volume, double lots, double volumeRate)
		{
			if (symBase.AccMethod == PosAccMethod.Netting)
			{
				switch (sym.CalcMode)
				{
					case CalculationMode.ExchangeStocks:
					case CalculationMode.ExchangeBounds:
						return CalcExchangeProfit(symBase, sym, type, price, volume, volumeRate);
					case CalculationMode.Collateral:
						return CalcCollateralProfit(symBase, sym, price, volume);
				}
			}
			else
			{
				switch (sym.CalcMode)
				{
					case CalculationMode.Forex:
					case CalculationMode.CalcMode5:
						CalcForexProfit(symBase, sym, openPrice, volume, lots, volumeRate);
						break;
					case CalculationMode.Collateral:
						return 0;
				}
			}
			return CalcBaseProfit(symBase, sym, type, openPrice, volume, volumeRate);
		}

		private double CalcBaseProfit(SymBaseInfo symBase, SymbolInfo sym, OrderType type, double openPrice, double volume, double volumeRate)
		{
			throw new NotImplementedException();
		}

		private double CalcCollateralProfit(SymBaseInfo symBase, SymbolInfo sym, double price, double volume)
		{
			throw new NotImplementedException();
		}

		private double CalcExchangeProfit(SymBaseInfo symBase, SymbolInfo sym, OrderType type, double price, double volume, double volumeRate)
		{
			throw new NotImplementedException();
		}

		struct ProfitStruct                                  //sizeof 0x66 d
		{
			public string Currency;        //0
			public double Lots;             //40
			public int SymDigits;          //48
			public int BaseDigits;         //49
			public double Profit;               //4A
			public double ProfitRate;           //52
			public double Margin;               //5A
			private int s62;
		}

		Dictionary<string, ProfitStruct> Profits = new Dictionary<string, ProfitStruct>();

		void CalcForexProfit(SymBaseInfo symBase, SymbolInfo sym, double price, double volume, double lots, double volumeRate)
		{
			var baseCurrency = sym.Currency;
			baseCurrency = baseCurrency.Substring(0, 3);
			ProfitStruct profit;
			if (Profits.ContainsKey(baseCurrency))
				profit = Profits[baseCurrency];
			else
			{
				profit = new ProfitStruct();
				profit.SymDigits = sym.Digits;
				profit.BaseDigits = symBase.Digits;
			}
			if (volume>0)
			{
				double vl = Math.Round(volume * lots, 8);
				double nlots = profit.Lots + vl;
				if (nlots > 0)
				{
					profit.ProfitRate = (profit.Lots * profit.ProfitRate + vl * volumeRate) / nlots;
					profit.Lots = nlots;
				}
				else
				{
					profit.ProfitRate = 0;
					profit.Lots = 0;
				}
			}
			profit.Profit = Math.Round(profit.ProfitRate * profit.Lots / Api.Account.Leverage, sym.Digits);
			var quoteCurrency = sym.Currency.Substring(3, 3);
			if (Profits.ContainsKey(quoteCurrency))
				profit = Profits[quoteCurrency];
			else
			{
				profit = new ProfitStruct();
				profit.SymDigits = sym.Digits;
				profit.BaseDigits = symBase.Digits;
			}
			if (volume > 0)
			{
				double vl = Math.Round(-(volume * lots * price), 8);
				double nlots = Math.Round(profit.Lots + vl, 8);
				if (nlots > 0)
				{
					profit.ProfitRate = (profit.Lots * profit.ProfitRate + vl * volumeRate / price) / nlots;
					profit.Lots = nlots;
				}
				else
				{
					profit.ProfitRate = 0;
					profit.Lots = 0;
				}
			}
			profit.Profit = Math.Round(profit.Lots * profit.ProfitRate / Api.Account.Leverage, sym.Digits);
		}

	for (int i = 0; i < deals.Count; i++)
			{
				var deal = deals[i];
				SymbolInfo sym;
				try
				{
					sym = Api.Symbols.GetInfo(deal.Symbol);
				}
				catch (Exception ex)
				{
					continue;
				}
				//double dir;
				//if (deal.Type == OrderType.Buy)
				//	dir = 1.0;
				//else if (deal.Type == OrderType.Sell)
				//	dir = -1.0;
				//else
				//	continue;
				while (Api.GetQuote(deal.Symbol) == null)
					Thread.Sleep(1);
				double price = Api.GetQuote(deal.Symbol).Ask;
				if (deal.Type == OrderType.Buy)
					price = Api.GetQuote(deal.Symbol).Bid;
				//CalcProfit(symBase, sym, deal.Type, deal.OpenPrice, price,
				//	deal.Lots * dir, deal.ContractSize, deal.VolumeRate);
				CalculateProfit(sym, deal.Type == OrderType.Buy, deal.Lots, deal.ContractSize, deal.OpenPrice, price, deal);
			}
			//vProfitBase::UpdateProfit(symBase, trdInfo);

 */
