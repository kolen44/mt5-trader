using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5.Internal
{
    class TradesInfo //sizeof 0x328 d
    {
        public ulong m_nLogin = 0; //0
        private string s8 = new string(new char[16]);
        public int m_nDigits = 0; //18
        private string s1C = new string(new char[36]);
        public double m_dBalance = 0; //40
        public double m_dCredit = 0; //48
        public double m_dCommission = 0; //50
        public double m_dBlocked = 0; //58
        private string s60 = new string(new char[24]);
        public double m_dMargin; //78
        public double m_dMarginFree = 0; //80
        public double m_dMarginLevel = 0; //88
        public int m_nLeverage; //90
        //private int s94;
        public double m_dMarginInitial = 0; //98
        public double m_dMarginMaintenance = 0; //A0
        private string sA8 = new string(new char[16]);
        public double m_dOrderProfit = 0; //B8
        public double m_dSwap = 0; //C0
        public double m_dOrderCommission = 0; //C8
        public double m_dProfit = 0; //D0
        public double m_dEquity = 0; //D8
        public double m_dAssets; //E0
        public double m_dLiabilities; //E8
        public double m_dCollateral; //F0
        private string sF8 = new string(new char[136]);
        //private int s180;
        private string s184 = new string(new char[316]);
        //private double s2C0;
        //private int s2C8;
        private string s2CC = new string(new char[88]);
    }

    class OrderMargin
    {
        MT5API Api;

        public OrderMargin(MT5API api)
        {
            Api = api;
        }

        async Task<double> GetBidRate(string pCurrency1, string pCurrency2)
        {
            string cur = pCurrency1 + pCurrency2;
            if (Api.Symbols.Exist(cur))
            {
                if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
                    return Double.NaN;
                return (await Api.GetQuoteInternal(cur, 0)).Bid;
            }
            cur = pCurrency2 + pCurrency1;
            if (Api.Symbols.Exist(cur))
            {
                if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
                    return Double.NaN;
                return 1 / (await Api.GetQuoteInternal(cur, 0)).Ask;
            }
            return 0;
        }

        public async Task<double> GetBidProfitRate(string pCurrency1, string pCurrency2)
        {
            if (string.IsNullOrWhiteSpace(pCurrency1))
                throw new Exception("pCurrency1 is null or empty");
            if (string.IsNullOrWhiteSpace(pCurrency2))
                throw new Exception("pCurrency2 is null or empty");
            if (string.Compare(pCurrency1, pCurrency2) != 0 || IsRubleCurrency(pCurrency1, pCurrency2))
            {
                return 1.0;
            }
            double rate = await GetBidRate(pCurrency1, pCurrency2);
            if (Double.IsNaN(rate))
                return 0;
            if (rate != 0)
            {
                return rate;
            }
            double toUSD = await GetBidRate(pCurrency1, "USD");
            if (Double.IsNaN(toUSD))
                return 0;
            if (toUSD == 0)
            {
                return 0;
            }
            double fromUsd = await GetBidRate("USD", pCurrency2);
            if (Double.IsNaN(fromUsd))
                return 0;
            if (fromUsd == 0)
            {
                return 0;
            }
            return toUSD * fromUsd;
        }

        public static bool IsRubleCurrency(string pCurrency1, string pCurrency2)
        {
            string[] sCurrency = { "RUB", "RUR" };
            for (int i = 0; i < 1; i++)
            {
                if (string.Compare(pCurrency1, sCurrency[i]) != 0 && string.Compare(pCurrency2, sCurrency[i + 1]) != 0)
                {
                    return true;
                }
                if (string.Compare(pCurrency2, sCurrency[i]) != 0 && string.Compare(pCurrency1, sCurrency[i + 1]) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        protected async Task<double> GetAskRate(string pCurrency1, string pCurrency2)
        {
            string cur = pCurrency1 + pCurrency2;
            if (Api.Symbols.Exist(cur))
            {
                if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
                    return Double.NaN;
                return (await Api.GetQuoteInternal(cur, 0)).Ask;
            }
            cur = pCurrency2 + pCurrency1;
            if (Api.Symbols.Exist(cur))
            {
                if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
                    return Double.NaN;
                return 1 / (await Api.GetQuoteInternal(cur, 0)).Bid;
            }
            return 0;
        }

        public async Task<double> GetAskProfitRate(string pCurrency1, string pCurrency2)
        {
            if (string.IsNullOrWhiteSpace(pCurrency1))
                throw new Exception("pCurrency1 is null or empty");
            if (string.IsNullOrWhiteSpace(pCurrency2))
                throw new Exception("pCurrency2 is null or empty");
            if (string.Compare(pCurrency1, pCurrency2) != 0 || IsRubleCurrency(pCurrency1, pCurrency2))
            {
                return 1.0;
            }
            double rate = await GetAskRate(pCurrency1, pCurrency2);
            if (Double.IsNaN(rate))
                return 0;
            if (rate != 0)
            {
                return rate;
            }
            double toUSD = await GetAskRate(pCurrency1, "USD");
            if (Double.IsNaN(toUSD))
                return 0;
            if (toUSD == 0)
            {
                return 0;
            }
            double fromUSD = await GetAskRate("USD", pCurrency2);
            if (Double.IsNaN(fromUSD))
                return 0;
            if (fromUSD == 0)
            {
                return 0;
            }
            return toUSD * fromUSD;
        }

        double RoundAdd(double value1, double value2, int digits)
        {
            return Math.Round(value1 + value2, digits);
        }

        protected async Task<double> GetPrice(Order order)
        {
            if (Api.GetQuoteInternal(order.Symbol, Api.GetQuoteTimeoutMs) == null)
                return 0;
            if (order.OrderType == OrderType.Buy)
            {
                return (await Api.GetQuoteInternal(order.Symbol, 0)).Bid;
            }
            if (order.OrderType == OrderType.Sell)
            {
                return (await Api.GetQuoteInternal(order.Symbol, 0)).Ask;
            }
            return 0;
        }

        double m_dContractSize = 0;
        double m_dMaintenanceMargin = 0;
        double m_dInitialMargin = 0;

        double IntToDouble(int value)
        {
            double _DP2to32 = 4.294967296e9;
            double res = (double)value;
            if (value < 0)
                res += _DP2to32;
            return res;
        }

        double m_dTickSize = 0;
        double m_dTickValue = 0;
        double m_dSettlementPrice = 0;
        double m_dUpperLimit = 0;
        double m_dLowerLimit = 0;
        double s1F0 = 0;
        double m_dFaceValue = 0;
        int sE8 = 0;

        public double CalcDefMargin(int nLeverage, OrderType type, bool bInitialMargin, bool bPrice, double lots, double price, SymbolInfo sym)
        {
            double initialMargin = m_dInitialMargin;
            if (!bInitialMargin && m_dMaintenanceMargin != 0)
            {
                initialMargin = m_dMaintenanceMargin;
            }
            double leverage = IntToDouble(nLeverage);
            double margin = 0;
            switch (sym.CalcMode)
            {
                case CalculationMode.Forex:
                    if (initialMargin > 0)
                    {
                        margin = (lots * initialMargin) / (m_dContractSize * leverage);
                    }
                    else
                    {
                        margin = lots / leverage;
                    }
                    break;
                case CalculationMode.Futures:
                case CalculationMode.ExchangeFutures:
                case CalculationMode.ExchangeMarginOption:
                    margin = lots * initialMargin / m_dContractSize;
                    break;
                case CalculationMode.CFD:
                case CalculationMode.ExchangeStocks:
                    if (initialMargin > 0)
                    {
                        margin = lots * initialMargin / m_dContractSize;
                    }
                    else
                    {
                        margin = lots * price;
                    }
                    break;
                case CalculationMode.CFDIndex:
                    if (initialMargin > 0)
                    {
                        margin = lots * initialMargin / m_dContractSize;
                    }
                    else if (m_dTickSize != 0)
                    {
                        margin = lots * price / m_dTickSize * m_dTickValue;
                    }
                    break;
                case CalculationMode.CFDLeverage:
                    if (initialMargin > 0)
                    {
                        margin = (lots * initialMargin) / (m_dContractSize * leverage);
                    }
                    else
                    {
                        margin = lots * price / leverage;
                    }
                    break;
                case CalculationMode.CalcMode5:
                    if (initialMargin > 0)
                    {
                        margin = lots * initialMargin / m_dContractSize;
                    }
                    else
                    {
                        margin = lots;
                    }
                    break;
                case CalculationMode.FORTSFutures:
                    if (m_dTickSize > 0)
                    {
                        double rate = (m_dTickValue / m_dTickSize) * (s1F0 * 0.01 + 1.0);
                        switch (type)
                        {
                            case OrderType.Buy:
                                if (bPrice)
                                {
                                    margin = m_dInitialMargin + (price - m_dSettlementPrice) * rate;
                                }
                                else
                                {
                                    margin = m_dInitialMargin + (m_dUpperLimit - m_dSettlementPrice) * rate;
                                }
                                margin *= lots / m_dContractSize;
                                break;
                            case OrderType.Sell:
                                if (bPrice)
                                {
                                    margin = m_dMaintenanceMargin + (m_dSettlementPrice - price) * rate;
                                }
                                else
                                {
                                    margin = m_dMaintenanceMargin + (m_dSettlementPrice - m_dLowerLimit) * rate;
                                }
                                margin *= lots / m_dContractSize;
                                break;
                            case OrderType.BuyLimit:
                            case OrderType.BuyStopLimit:
                                margin = m_dInitialMargin + (price - m_dSettlementPrice) * rate;
                                margin *= lots / m_dContractSize;
                                break;
                            case OrderType.SellLimit:
                            case OrderType.SellStopLimit:
                                margin = m_dMaintenanceMargin + (m_dSettlementPrice - price) * rate;
                                margin *= lots / m_dContractSize;
                                break;
                            case OrderType.BuyStop:
                                margin = m_dInitialMargin + (m_dUpperLimit - m_dSettlementPrice) * rate;
                                margin *= lots / m_dContractSize;
                                break;
                            case OrderType.SellStop:
                                margin = m_dInitialMargin + (m_dSettlementPrice - m_dLowerLimit) * rate;
                                margin *= lots / m_dContractSize;
                                break;
                            default:
                                margin = lots * initialMargin / m_dContractSize;
                                break;
                        }
                    }
                    break;
                case CalculationMode.ExchangeBounds:
                    if (initialMargin > 0)
                    {
                        margin = lots * initialMargin / m_dContractSize;
                    }
                    else
                    {
                        margin = Math.Round(lots * price * m_dFaceValue * 0.01, sE8);
                    }
                    break;
                case CalculationMode.Collateral:
                    break;
            }
            return margin;
        }

        private ulong m_lVolume = 0; //218
        private double m_dLots = 0; //220
        private double m_dOpenPrice = 0; //228
        private double m_dPrice = 0; //230
        private double m_dMargin = 0; //238
        private double m_dBuyMargin = 0; //240
        private double m_dSellMargin = 0; //248
        private ulong m_lBuyVolume = 0; //250
        private ulong m_lSellVolume = 0; //258
        //private bool m_bOrder = false; //260
        private double m_dTradeMargin; //268
        private ulong m_lTradeVolume; //270
        //private ulong m_lSpreadVolume = 0; //278
        private OrderType m_TradeType; //280
        private double m_dMarginRate; //288
        double s1E8 = 0;

        public async Task<double> GetTradeMargin(SymbolInfo sym, Order order, TradesInfo trdInfo)
        {
            double buyMargin = m_dBuyMargin;
            double sellMargin = m_dSellMargin;
            string m_sProfitCurrency = Api.Symbols.Base.Currency;
            var m_Type = order.OrderType;

            m_dTradeMargin = 0;
            if (sym.CalcMode == CalculationMode.Collateral)
            {
                m_lTradeVolume = 0;
                m_TradeType = OrderType.Buy;
                m_dMarginRate = 0;
                if (order.OrderType == OrderType.Buy)
                {
                    double profitRate = await GetAskProfitRate(m_sProfitCurrency, sym.Currency);
                    double price = m_dPrice;
                    if (price == 0)
                    {
                        price = await GetPrice(order);
                    }
                    double volume = Math.Round(m_dLots * price, sym.Digits);
                    buyMargin = Math.Round(volume * profitRate * s1E8, sym.Digits);
                    trdInfo.m_dAssets = RoundAdd(trdInfo.m_dAssets, buyMargin, sym.Digits);
                    trdInfo.m_dCollateral = RoundAdd(trdInfo.m_dCollateral, buyMargin, sym.Digits);
                }
                if (order.OrderType == OrderType.Sell)
                {
                    double profitRate = await GetBidProfitRate(m_sProfitCurrency, sym.Currency);
                    double price = m_dPrice;
                    if (price == 0)
                    {
                        price = await GetPrice(order);
                    }
                    double volume = Math.Round(m_dLots * price, sym.Digits);
                    sellMargin = -Math.Round(volume * profitRate, sym.Digits);
                    trdInfo.m_dLiabilities = RoundAdd(trdInfo.m_dLiabilities, sellMargin, sym.Digits);
                    trdInfo.m_dCollateral = RoundAdd(trdInfo.m_dCollateral, sellMargin, sym.Digits);
                }
                return m_dTradeMargin;
            }

            if (m_Type == OrderType.Buy)
            {
                buyMargin += m_dMargin;
                if (sym.CalcMode == CalculationMode.FORTSFutures)
                {
                    sellMargin -= Math.Round(CalcDefMargin(Api.Account.Leverage, OrderType.Sell, false, true, m_dLots, m_dOpenPrice, sym), sym.Digits);
                }
                else if (m_dSellMargin != 0 && m_lSellVolume != 0)
                {
                    sellMargin -= Math.Round(m_dSellMargin / ULL2DBL(m_lSellVolume) * ULL2DBL(m_lVolume), trdInfo.m_nDigits);
                }
            }
            if (m_Type == OrderType.Sell)
            {
                sellMargin += m_dMargin;
                if (sym.CalcMode == CalculationMode.FORTSFutures)
                {
                    buyMargin -= Math.Round(CalcDefMargin(Api.Account.Leverage, OrderType.Buy, false, true, m_dLots, m_dOpenPrice, sym), sym.Digits);
                }
                else if (m_dBuyMargin != 0 && m_lBuyVolume != 0)
                {
                    buyMargin -= Math.Round(m_dBuyMargin / ULL2DBL(m_lBuyVolume) * ULL2DBL(m_lVolume), trdInfo.m_nDigits);
                }
            }

            double margin = Math.Max(buyMargin, sellMargin);
            m_dTradeMargin = margin;
            m_lTradeVolume = 0;
            m_dMarginRate = 0;
            m_TradeType = OrderType.Buy;
            ulong buyVolume = m_lBuyVolume;
            ulong sellVolume = m_lSellVolume;
            if (m_lVolume != 0)
            {
                if (m_Type == OrderType.Buy)
                {
                    buyVolume += m_lVolume;
                }
                if (m_Type == OrderType.Sell)
                {
                    sellVolume += m_lVolume;
                }
            }
            if (buyVolume > sellVolume)
            {
                m_TradeType = OrderType.Buy;
                m_lTradeVolume = buyVolume;
            }
            else if (buyVolume < sellVolume)
            {
                m_TradeType = OrderType.Sell;
                m_lTradeVolume = sellVolume;
            }
            else
            {
                m_TradeType = m_Type;
                m_lTradeVolume = buyVolume;
            }
            if (m_lTradeVolume != 0)
            {
                m_dMarginRate = margin / ULL2DBL(m_lTradeVolume);
            }
            trdInfo.m_dMargin = RoundAdd(trdInfo.m_dMargin, margin, sym.Digits);
            return m_dTradeMargin;
        }

        double ULL2DBL(ulong value)
        {
            return (double)value;
        }


    }
}
//	double m_dBuyMargin;
//	double m_dSellMargin;

//	double GetAskProfitRate(string pCurrency1, string pCurrency2, SymbolInfo pSym)
//	{
//		if (string.IsNullOrWhiteSpace(pCurrency1))
//			throw new Exception("pCurrency1 is null or empty");
//		if (string.IsNullOrWhiteSpace(pCurrency2))
//			throw new Exception("pCurrency2 is null or empty");
//		if (pCurrency1 == pCurrency1)
//			return 1;
//		double rate = GetAskRate(pCurrency1, pCurrency2, pSym);
//		if (rate)
//			return rate;
//		double toUSD = GetAskRate(pCurrency1, L"USD", pSym);
//		if (!toUSD)
//			return 0;
//		double fromUSD = GetAskRate(L"USD", pCurrency2, pSym);
//		if (!fromUSD)
//			return 0;
//		return toUSD * fromUSD;
//	}

//	double GetAskRate(string pCurrency1, string pCurrency2, SymbolInfo pSym)
//	{
//		string currency = "";
//		if (OnRequestTick(pCurrency1, pCurrency2, pSym, currency))
//		{
//			double rate = GetAsk(currency);
//			if (rate > 0)
//				return rate;
//		}
//		if (OnRequestTick(pCurrency2, pCurrency1, pSym, currency))
//		{
//			double rate = GetBid(currency);
//			if (rate > 0)
//				return 1.0 / rate;
//		}
//		return 0;
//	}

//	bool OnRequestTick(wchar_t* pCurrency1, wchar_t* pCurrency2, vSymbol* pSym, wchar_t* pCurrency)
//	{
//		if (!pCurrency1 || !pCurrency2 || !pCurrency)
//			return false;
//		memcpy(pCurrency, pCurrency1, 3 * sizeof(wchar_t));
//		memcpy(&pCurrency[3], pCurrency2, 3 * sizeof(wchar_t));
//		if (pSym && ((pSym->m_SymInfo.m_CalcMode == vForex) || (pSym->m_SymInfo.m_CalcMode == vCalcMode5)))
//		{
//			memcpy(&pCurrency[6], &pSym->m_SymInfo.m_sCurrency[6], 13 * sizeof(wchar_t));
//			pCurrency[31] = 0;
//		}
//		else
//			pCurrency[6] = 0;
//		return m_arrAbsentSym.Find(&vText < 32 > (pCurrency), vText < 32 >::Compare) == NULL;
//	}


//	double GetTradeMargin(SymbolInfo sym, Order order)
//	{
//		double buyMargin = m_dBuyMargin;
//		double sellMargin = m_dSellMargin;
//		if (sym.CalcMode == CalculationMode.Collateral)
//		{
//			double m_dTradeMargin = 0;
//			double m_lTradeVolume = 0;
//			OrderType m_TradeType = OrderType.Buy;
//			double m_dMarginRate = 0;
//			if (order.OrderType ==  OrderType.Buy)

//				double profitRate = m_pQuotes->GetAskProfitRate(m_Sym.m_sProfitCurrency,
//					m_pSymBase->m_SymInfo.m_sCurrency, NULL, 0);
//				double price = m_dPrice;
//				if (!price)
//					price = GetPrice(m_Type);
//				double volume = Math.Round(m_dLots * price, m_pSymBase->m_SymInfo.m_nDigits);
//				buyMargin = Math.Round(volume * profitRate * m_Sym.s1E8, m_pSymBase->m_SymInfo.m_nDigits);
//				trd.m_dAssets = Math.Round(trd.m_dAssets + buyMargin, m_pSymBase->m_SymInfo.m_nDigits);
//				trd.m_dCollateral = Math.Round(trd.m_dCollateral + buyMargin, m_pSymBase->m_SymInfo.m_nDigits);
//			}
//			if (m_Type == vSell)
//			{
//				double profitRate = m_pQuotes->GetBidProfitRate(m_Sym.m_sProfitCurrency,
//					m_pSymBase->m_SymInfo.m_sCurrency, NULL, 0);
//				double price = m_dPrice;
//				if (!price)
//					price = GetPrice(m_Type);
//				double volume = Math.Round(m_dLots * price, m_pSymBase->m_SymInfo.m_nDigits);
//				sellMargin = Math.Round(volume * profitRate, m_pSymBase->m_SymInfo.m_nDigits);
//				trd.m_dLiabilities = Math.Round(trd.m_dLiabilities - sellMargin, m_pSymBase->m_SymInfo.m_nDigits);
//				trd.m_dCollateral = Math.Round(trd.m_dCollateral - sellMargin, m_pSymBase->m_SymInfo.m_nDigits);
//			}
//			return m_dTradeMargin;
//		}
//		m_dTradeMargin = 0;
//		if (m_lVolume)
//		{
//			if (m_Type == vBuy)
//			{
//				buyMargin += m_dMargin;
//				if (sellMargin && m_lSellVolume)
//					sellMargin -= Math.Round(sellMargin / LL2DBL(m_lSellVolume) * LL2DBL(m_lVolume), trd.m_nDigits);
//			}
//			if (m_Type == vSell)
//			{
//				sellMargin += m_dMargin;
//				if (buyMargin && m_lBuyVolume)
//					buyMargin -= Math.Round(buyMargin / LL2DBL(m_lBuyVolume) * LL2DBL(m_lVolume), trd.m_nDigits);
//			}
//		}
//		double margin = max(buyMargin, sellMargin);
//		m_dTradeMargin = margin;
//		m_lTradeVolume = 0;
//		m_dMarginRate = 0;
//		m_TradeType = vBuy;
//		LONGLONG buyVolume = m_lBuyVolume;
//		LONGLONG sellVolume = m_lSellVolume;
//		if (m_lVolume)
//		{
//			if (m_Type == vBuy)
//				buyVolume += m_lVolume;
//			if (m_Type == vSell)
//				sellVolume += m_lVolume;
//		}
//		if (buyVolume > sellVolume)
//		{
//			m_TradeType = vBuy;
//			m_lTradeVolume = buyVolume;
//		}
//		else if (buyVolume < sellVolume)
//		{
//			m_TradeType = vSell;
//			m_lTradeVolume = sellVolume;
//		}
//		else
//		{
//			m_TradeType = m_Type;
//			m_lTradeVolume = buyVolume;
//		}
//		if (m_lTradeVolume)
//			m_dMarginRate = margin / LL2DBL(m_lTradeVolume);
//		trd.m_dMargin = Math.Round(trd.m_dMargin + margin, m_pSymBase->m_SymInfo.m_nDigits);
//		return m_dTradeMargin;
//	}

//}

//      public void Calculate()
//{
//          var orders = Api.GetOpenedOrders();
//          double margin = 0;
//          for (int i = 0; i < orders.Length; i++)
//          {
//              Order order = orders[i];
//              //if (order.OrderType != OrderType.Buy && order.OrderType != OrderType.Sell)
//              //    continue;
//              var info = Api.Symbols.GetInfo(order.Symbol);
//              double delta_volume = 0;
//              double all_volume = 0;
//              double all_open_price = 0;
//              double all_margin_rate = 0;
//              double buy_volume = 0;
//              double buy_open_price = 0;
//              double buy_margin_rate = 0;
//              double sell_volume = 0;
//              double sell_open_price = 0;
//              double sell_margin_rate = 0;
//              string currency = order.Symbol;
//              for (; i < orders.Length; i++)
//              {
//                  order = orders[i];
//                  //if (order.OrderType != OrderType.Buy && order.OrderType != OrderType.Sell)
//                  //    continue;
//                  if (currency != order.Symbol)
//                      break;
//                  double volume = order.Lots;
//                  var marginRate = GetMarginRate(order.OrderType, false, order.Symbol);
//                  if (order.OrderType == OrderType.Buy)
//                  {
//                      buy_volume += volume;
//                      delta_volume += volume;
//                      buy_open_price += order.OpenPrice * volume;
//                      buy_margin_rate += marginRate * volume;
//                  }
//                  else
//                  {
//                      sell_volume += volume;
//                      delta_volume -= volume;
//                      sell_open_price += order.OpenPrice * volume;
//                      sell_margin_rate += marginRate * volume;
//                  }
//                  all_volume += volume;
//                  all_open_price += order.OpenPrice * volume;
//                  all_margin_rate += marginRate * volume;
//              }
//              margin += CalculateMargin(info, Api.Account.Leverage, false,
//                  all_volume, delta_volume, all_open_price, all_margin_rate,
//                  buy_volume, buy_open_price, buy_margin_rate,
//                  sell_volume, sell_open_price, sell_margin_rate);
//              i--;
//          }
//      }

//      double CalculateMargin(SymbolInfo info, int leverage, bool bFlag,
//                          double all_volume, double delta_volume, double all_open_price, double all_margin_rate,
//                          double buy_volume, double buy_open_price, double buy_margin_rate,
//                          double sell_volume, double sell_open_price, double sell_margin_rate)
//      {
//          double open_price = (all_volume > 0) ? all_open_price / all_volume : 0;
//          double margin_rate = (all_volume > 0) ? all_margin_rate / all_volume : 0;
//          double abs_delta_volume = Math.Abs(delta_volume);
//          double size, margin = 0;

//          switch (info.CalcMode)
//          {
//              case CalculationMode.Forex:
//                  if (all_volume > abs_delta_volume)
//                  {
//                      if (_Account.unused_rights[0] != 0)
//                      {
//                          double margin_buy = buy_margin_rate / buy_volume * (buy_volume * info.contract_size / leverage / info.margin_divider);
//                          double margin_sell = sell_margin_rate / sell_volume * (sell_volume * info.contract_size / leverage / info.margin_divider);
//                          return (margin_buy > margin_sell) ? margin_buy : margin_sell;
//                      }
//                      margin = (all_volume - abs_delta_volume) * info.margin_hedged / leverage / info.margin_divider;
//                  }
//                  if (delta_volume == 0)
//                      break;
//                  size = (info.margin_initial > 0) ? info.margin_initial : info.contract_size;
//                  return (margin + abs_delta_volume * size / leverage / info.margin_divider) * margin_rate;
//              case MarginMode.CFD:
//                  if (delta_volume != 0)
//                      margin = abs_delta_volume *
//                          ((info.margin_initial > 0) ? info.margin_initial : open_price * info.contract_size) / info.margin_divider;
//                  if (abs_delta_volume >= all_volume)
//                      break;
//                  if (_Account.unused_rights[0] != 0)
//                  {
//                      double margin_buy = (info.margin_initial > 0) ? buy_volume : buy_open_price;
//                      margin_buy *= info.margin_hedged;
//                      margin_buy /= info.margin_divider;
//                      double margin_sell = (info.margin_initial > 0) ? sell_volume : sell_open_price;
//                      margin_sell *= info.margin_hedged;
//                      margin_sell /= info.margin_divider;
//                      return (margin_buy > margin_sell) ? margin_buy : margin_sell;
//                  }
//                  if (info.margin_initial > 0)
//                      return (margin + (all_volume - abs_delta_volume) * info.margin_hedged / info.margin_divider) * margin_rate;
//                  return (margin + open_price * (all_volume - abs_delta_volume) * info.margin_hedged / info.margin_divider) * margin_rate;
//              case MarginMode.Futures:
//                  if (delta_volume != 0)
//                  {
//                      margin = ((info.margin_initial > 0) && bFlag) ? info.margin_initial : info.margin_maintenance;
//                      margin *= abs_delta_volume;
//                      margin /= info.margin_divider;
//                  }
//                  if (abs_delta_volume >= all_volume)
//                      break;
//                  if (_Account.unused_rights[0] != 0)
//                  {
//                      double margin_buy = buy_volume * info.margin_hedged / info.margin_divider;
//                      double margin_sell = sell_volume * info.margin_hedged / info.margin_divider;
//                      return (margin_buy > margin_sell) ? margin_buy : margin_sell;
//                  }
//                  return (margin + (all_volume - abs_delta_volume) * info.margin_hedged / info.margin_divider) * margin_rate;
//              case MarginMode.CfdIndex:
//                  if ((info.tick_size == 0) || (info.tick_value == 0))
//                      break;
//                  if (delta_volume != 0)
//                  {
//                      margin = ((info.margin_initial > 0) ? info.margin_initial :
//                          open_price * info.contract_size / info.tick_size * info.tick_value) * abs_delta_volume;
//                      margin /= info.margin_divider;
//                  }
//                  if (abs_delta_volume >= all_volume)
//                      break;
//                  if (_Account.unused_rights[0] != 0)
//                  {
//                      double margin_buy = (info.margin_initial > 0) ? buy_volume * info.margin_hedged :
//                          buy_open_price * info.margin_hedged / info.tick_size * info.tick_value;
//                      margin_buy /= info.margin_divider;
//                      double margin_sell = (info.margin_initial > 0) ? sell_volume * info.margin_hedged :
//                          sell_open_price * info.margin_hedged / info.tick_size * info.tick_value;
//                      margin_sell /= info.margin_divider;
//                      return (margin_buy > margin_sell) ? margin_buy : margin_sell;
//                  }
//                  if (info.margin_initial > 0)
//                      return (margin + (all_volume - abs_delta_volume) * info.margin_hedged / info.margin_divider) * margin_rate;
//                  return (margin + open_price * (all_volume - abs_delta_volume) * info.margin_hedged / info.tick_size *
//                      info.tick_value / info.margin_divider) * margin_rate;
//              case MarginMode.CfdLeverage:
//                  if (delta_volume != 0)
//                  {
//                      margin = (info.margin_initial > 0) ? info.margin_initial : open_price * info.contract_size;
//                      margin *= abs_delta_volume;
//                      margin /= leverage;
//                      margin /= info.margin_divider;
//                  }
//                  if (abs_delta_volume >= all_volume)
//                      break;
//                  if (_Account.unused_rights[0] != 0)
//                  {
//                      double margin_buy = (info.margin_initial > 0) ? buy_volume : buy_open_price;
//                      margin_buy *= info.margin_hedged;
//                      margin_buy /= leverage;
//                      margin_buy /= info.margin_divider;
//                      double margin_sell = (info.margin_initial > 0) ? sell_volume : sell_open_price;
//                      margin_sell *= info.margin_hedged;
//                      margin_sell /= leverage;
//                      margin_sell /= info.margin_divider;
//                      return (margin_buy > margin_sell) ? margin_buy : margin_sell;
//                  }
//                  if (info.margin_initial > 0)
//                      return (margin + (all_volume - abs_delta_volume) * info.margin_hedged / leverage / info.margin_divider) * margin_rate;
//                  return (margin + open_price * (all_volume - abs_delta_volume) * info.margin_hedged / leverage / info.margin_divider) * margin_rate;
//          }
//          return margin * margin_rate;
//      }

//      double GetMarginRate(OrderType type, bool init, string symbol)
//      {
//          var initMarginRate = Api.Symbols.GetGroup(symbol).InitMarginRate;
//          var maintainceMarginRate = Api.Symbols.GetGroup(symbol).MntnMarginRate;
//          double im = 0;
//          double mm = 0;
//          switch (type)
//          {
//              case OrderType.Buy:
//                  im = initMarginRate[0];
//                  mm = maintainceMarginRate[0];
//                  break;
//              case OrderType.Sell:
//                  im = initMarginRate[1];
//                  mm = maintainceMarginRate[1];
//                  break;
//              case OrderType.BuyLimit:
//                  im = initMarginRate[2];
//                  mm = maintainceMarginRate[2];
//                  break;
//              case OrderType.SellLimit:
//                  im = initMarginRate[3];
//                  mm = maintainceMarginRate[3];
//                  break;
//              case OrderType.BuyStop:
//                  im = initMarginRate[4];
//                  mm = maintainceMarginRate[4];
//                  break;
//              case OrderType.SellStop:
//                  im = initMarginRate[5];
//                  mm = maintainceMarginRate[5];
//                  break;
//              case OrderType.BuyStopLimit:
//                  im = initMarginRate[6];
//                  mm = maintainceMarginRate[6];
//                  break;
//              case OrderType.SellStopLimit:
//                  im = initMarginRate[7];
//                  mm = maintainceMarginRate[7];
//                  break;
//          }
//          return init ? im : mm;
//      }