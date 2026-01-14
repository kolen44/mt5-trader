using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5.Internal
{
	class HedStatInfo //sizeof 0x30 d
	{
		public ulong m_lVolume; //0
		public ulong m_lOrderVolume; //8
		public double m_dLots; //10
		public double m_dPrice; //18
		public double m_dVolume; //20
		public double m_dMarginRate; //28

	}
	class SymbolMargin
	{

		//static void Calculate(TradesInfo trdInfo, Order[] orders, Order pOrder)
		//{
		//	bool bOrder = pOrder != null;
		//	foreach (var pCurOrder in orders)
		//	{
		//		HedStatRec pRec = GetTradeRecord(trdInfo, ref pCurOrder.m_sCurrency);
		//		if (pRec == null)
		//		{
		//			continue;
		//		}
		//		if (pOrder != null && (pOrder.m_lTicketNumber == pCurOrder.m_lTicketNumber))
		//		{
		//			pRec.AcceptOrder(pOrder, bOrder);
		//			if (RoundCompare(trdInfo.m_nDigits, pCurOrder.s170, pOrder.s170))
		//			{
		//				trdInfo.m_dCommission = RoundAdd(trdInfo.m_dCommission, (pOrder.s170 - pCurOrder.s170), trdInfo.m_nDigits);
		//			}
		//			if (RoundCompare(trdInfo.m_nDigits, pCurOrder.s178, pOrder.s178))
		//			{
		//				trdInfo.m_dCommission = RoundAdd(trdInfo.m_dCommission, (pOrder.s178 - pCurOrder.s178), trdInfo.m_nDigits);
		//			}
		//			//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
		//			//ORIGINAL LINE: m_Order = *pOrder;
		//			m_Order.CopyFrom(pOrder);
		//			pOrder = null;
		//		}
		//		else
		//		{
		//			pRec.AcceptOrder(pCurOrder, bOrder);
		//		}
		//	}

		//	if (pOrder == null)
		//	{
		//		return;
		//	}
		//	vHedStatRec pRec = GetTradeRecord(trdInfo, ref pOrder.m_sCurrency);
		//	if (pRec == null)
		//	{
		//		return;
		//	}
		//	pRec.AcceptOrder(pOrder, bOrder);
		//	if (pOrder.s170)
		//	{
		//		trdInfo.m_dCommission = RoundAdd(trdInfo.m_dCommission, pOrder.s170, trdInfo.m_nDigits);
		//	}
		//	if (pOrder.s178)
		//	{
		//		trdInfo.m_dCommission = RoundAdd(trdInfo.m_dCommission, pOrder.s178, trdInfo.m_nDigits);
		//	}
		//	//C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
		//	//ORIGINAL LINE: m_Order = *pOrder;
		//	m_Order.CopyFrom(pOrder);
		//}

		MT5API Api;
		string Symbol;
		HedStatInfo[] m_Deal = new HedStatInfo[2];
		HedStatInfo[] m_Order = new HedStatInfo[8];
		double[] m_dMntnMarginRate;
		double[] m_dInitMarginRate;
		double m_dInitialMargin;
		double m_dMaintenanceMargin;
		TradesInfo m_pTradesInfo = new TradesInfo();
		double m_dContractSize;
		double s160;
		double s1E8;
		double s1F0;
		private int sE8;
		private int sEC;
		private int sF0;
		private int sF4;
		private CalculationMode m_CalcMode;
		private double m_dTickSize;
		private double m_dTickValue;
		private double m_dFaceValue;
		private double m_dTradeMargin;
		private string m_sProfitCurrency;
		private SymbolInfo m_SymInfo;
		private string m_sCurrency;
		private int s148;
		private string m_sMarginCurrency;

		public SymbolMargin(MT5API api, string symbol)
		{
			Api = api;
			Symbol = symbol;
			var gr = api.Symbols.GetGroup(symbol);
			var sym = api.Symbols.GetInfo(symbol);
			m_dContractSize = sym.ContractSize;
			m_dInitMarginRate = gr.InitMarginRate;
			m_dMntnMarginRate = gr.MntnMarginRate;
			m_dInitialMargin = gr.InitialMargin;
			m_dMaintenanceMargin = gr.MaintenanceMargin;
			s160 = gr.HedgedMargin;
			s1E8 = gr.s3D4;
			s1F0 = gr.s3E4;
			sE8 = sym.s468;
			sEC = sym.Precision;
			sF0 = gr.DeviationRate;
			sF4 = gr.RoundRate;
			m_CalcMode = sym.CalcMode;
			m_dTickSize = sym.TickSize;
			m_dTickValue = sym.TickValue;
			m_dFaceValue = sym.FaceValue;
			m_Deal[0] = new HedStatInfo();
			m_Deal[1] = new HedStatInfo();
			for (int i = 0; i < 8; i++)
				m_Order[i] = new HedStatInfo();
			m_pTradesInfo.m_nLeverage = api.Account.Leverage;
			m_sProfitCurrency = api.Symbols.Base.Currency;
			m_SymInfo = sym;
			m_sCurrency = symbol;
			s148 = gr.s340;
			m_sMarginCurrency = sym.MarginCurrency;
		}

		public bool AcceptDeal(DealInternal deal)
		{
			if (deal.Type == DealType.DealBuy)
			{
				m_Deal[0].m_lVolume += deal.Volume;
				m_Deal[0].m_dLots += ULL2DBL(deal.Volume) * deal.OpenPrice;
				m_Deal[0].m_dVolume += ULL2DBL(deal.Volume) * deal.VolumeRate;
				m_Deal[0].m_dPrice = deal.Price;
				m_Deal[0].m_dMarginRate = m_dMntnMarginRate[0] != 0 ? m_dMntnMarginRate[0] : m_dInitMarginRate[0];
				return true;
			}
			if (deal.Type == DealType.DealSell)
			{
				m_Deal[1].m_lVolume += deal.Volume;
				m_Deal[1].m_dLots += ULL2DBL(deal.Volume) * deal.OpenPrice;
				m_Deal[1].m_dVolume += ULL2DBL(deal.Volume) * deal.VolumeRate;
				m_Deal[1].m_dPrice = deal.Price;
				m_Deal[1].m_dMarginRate = m_dMntnMarginRate[1] != 0 ? m_dMntnMarginRate[1] : m_dInitMarginRate[1];
				return true;
			}
			return false;
		}

		public async Task<bool> AcceptOrder(OrderInternal order, bool bOrder)
		{
			if (m_CalcMode == CalculationMode.Collateral)
			{
				return false;
			}
			OrderType type = order.Type;
			if (order.IsAssociativeDealOrder() || (type == OrderType.CloseBy))
			{
				return true;
			}
			if (bOrder)
			{
				if ((order.s1C8 == 1) && (order.IsLimitOrder() || order.IsStopOrder()))
				{
					type = order.IsBuyOrder() ? OrderType.Buy : OrderType.Sell;
				}
				else if ((order.s1C8 == 2) && order.IsStopLimitOrder())
				{
					type = order.IsBuyOrder() ? OrderType.BuyLimit : OrderType.SellLimit;
				}
			}
			double mr = GetMarginRate(type, true, m_sCurrency);
			if (mr <= 0)
			{
				return true;
			}
			double price = order.OpenPrice;
			if (price == 0 && ((type == OrderType.Buy) || (type == OrderType.Sell)))
			{
				price = order.Price;
			}
			if ((type == OrderType.BuyStopLimit) || (type == OrderType.SellStopLimit))
			{
				price = order.StopLimitPrice;
			}
			double profitRate;
			if ((type == OrderType.Buy) || (type == OrderType.BuyStop) || (type == OrderType.BuyLimit) || (type == OrderType.BuyStopLimit))
			{
				profitRate = order.ProfitRate;
				if (profitRate == 0)
				{
					profitRate = await GetAskProfitRate(m_sMarginCurrency, m_sCurrency, price);
				}
				if (profitRate <= 0)
				{
					return true;
				}
			}
			else if ((type == OrderType.Sell) || (type == OrderType.SellStop) || (type == OrderType.SellLimit) || (type == OrderType.SellStopLimit))
			{
				profitRate = order.ProfitRate;
				if (profitRate == 0)
				{
					profitRate = await GetBidProfitRate(m_sMarginCurrency, m_sCurrency, price);
				}
				if (profitRate <= 0)
				{
					return true;
				}
			}
			else
			{
				return false;
			}
			m_Order[(int)type].m_lVolume += order.RequestVolume;
			m_Order[(int)type].m_dLots += ULL2DBL(order.RequestVolume) * price;
			m_Order[(int)type].m_dVolume += ULL2DBL(order.RequestVolume) * profitRate;
			m_Order[(int)type].m_dMarginRate = mr;
			return true;
		}

		public async Task<double> GetTradeMargin()
		{
			TradesInfo trdInfo = m_pTradesInfo;
			m_dTradeMargin = 0;
			if (m_Deal[0].m_lVolume != 0)
			{
				double vr = 1.0 / ULL2DBL(m_Deal[0].m_lVolume);
				m_Deal[0].m_dLots *= vr;
				m_Deal[0].m_dVolume *= vr;
			}
			if (m_Deal[1].m_lVolume != 0)
			{
				double vr = 1.0 / ULL2DBL(m_Deal[1].m_lVolume);
				m_Deal[1].m_dLots *= vr;
				m_Deal[1].m_dVolume *= vr;
			}
			if (m_CalcMode == CalculationMode.Collateral)
			{
				//vTickRate rate = new vTickRate();
				if (m_Deal[0].m_lVolume != 0)
				{
					double profit = await GetAskProfitRate(m_sProfitCurrency, m_SymInfo.Currency);
					double price = m_Deal[0].m_dPrice;
					if (price == 0)
					{
						price = await GetBid(m_sCurrency);
					}
					double volume = AsLots(m_Deal[0].m_lVolume, m_dContractSize);
					double margin = Math.Round(Math.Round(volume * price, m_SymInfo.Digits) * profit * s1E8, m_SymInfo.Digits);
					trdInfo.m_dAssets = RoundAdd(trdInfo.m_dAssets, margin, m_SymInfo.Digits);
					trdInfo.m_dCollateral = RoundAdd(trdInfo.m_dCollateral, margin, m_SymInfo.Digits);
				}
				if (m_Deal[1].m_lVolume != 0)
				{
					double profit = await GetBidProfitRate(m_sProfitCurrency, m_SymInfo.Currency);
					double price = m_Deal[1].m_dPrice;
					if (price == 0)
					{
						price = await GetAsk(m_sCurrency);
					}
					double volume = AsLots(m_Deal[1].m_lVolume, m_dContractSize);
					double margin = -Math.Round(Math.Round(volume * price, m_SymInfo.Digits) * profit, m_SymInfo.Digits);
					trdInfo.m_dLiabilities = RoundAdd(trdInfo.m_dLiabilities, margin, m_SymInfo.Digits);
					trdInfo.m_dCollateral = RoundAdd(trdInfo.m_dCollateral, margin, m_SymInfo.Digits);
				}
				return m_dTradeMargin;
			}
			double buyMargin = 0;
			double sellMargin = 0;
			if ((s148 & 4) != 0)
			{
				for (int i = 0; i < 8; i++)
				{
					HedStatInfo stat = m_Order[i];
					if (stat.m_lVolume == 0)
					{
						continue;
					}
					double vr = 1.0 / ULL2DBL(stat.m_lVolume);
					stat.m_dLots *= vr;
					stat.m_dVolume *= vr;
					if ((i == (int)OrderType.Buy) || (i == (int)OrderType.BuyStop) || (i == (int)OrderType.BuyLimit) || (i == (int)OrderType.BuyStopLimit))
					{
						buyMargin += CalcHedMargin(m_pTradesInfo, stat.m_lVolume, true, false, stat.m_dLots) * stat.m_dVolume * stat.m_dMarginRate;
					}
					if ((i == (int)OrderType.Sell) || (i == (int)OrderType.SellStop) || (i == (int)OrderType.SellLimit) || (i == (int)OrderType.SellStopLimit))
					{
						sellMargin += CalcHedMargin(m_pTradesInfo, stat.m_lVolume, true, false, stat.m_dLots) * stat.m_dVolume * stat.m_dMarginRate;
					}
				}
				double buyDealMargin = 0;
				if (m_Deal[0].m_lVolume != 0)
				{
					buyDealMargin = CalcHedMargin(m_pTradesInfo, m_Deal[0].m_lVolume, false, false, m_Deal[0].m_dLots) * m_Deal[0].m_dVolume * m_Deal[0].m_dMarginRate;
				}
				double sellDealMargin = 0;
				if (m_Deal[1].m_lVolume != 0)
				{
					sellDealMargin = CalcHedMargin(m_pTradesInfo, m_Deal[1].m_lVolume, false, false, m_Deal[1].m_dLots) * m_Deal[1].m_dVolume * m_Deal[1].m_dMarginRate;
				}
				buyMargin += buyDealMargin;
				sellMargin += sellDealMargin;
				m_dTradeMargin = Math.Max(buyMargin, sellMargin);
			}
			else
			{
				double dealMargin = CalculateDealMargin();
				for (int i = 0; i < 8; i++)
				{
					HedStatInfo stat = m_Order[i];
					if (stat.m_lVolume == 0)
					{
						continue;
					}
					double vr = 1.0 / ULL2DBL(stat.m_lVolume);
					stat.m_dLots *= vr;
					stat.m_dVolume *= vr;
					if ((i == (int)OrderType.BuyStop) || (i == (int)OrderType.BuyLimit) || (i == (int)OrderType.BuyStopLimit))
					{
						buyMargin += CalcHedMargin(m_pTradesInfo, stat.m_lVolume, true, false, stat.m_dLots) * stat.m_dVolume * stat.m_dMarginRate;
					}
					if ((i == (int)OrderType.SellStop) || (i == (int)OrderType.SellLimit) || (i == (int)OrderType.SellStopLimit))
					{
						sellMargin += CalcHedMargin(m_pTradesInfo, stat.m_lVolume, true, false, stat.m_dLots) * stat.m_dVolume * stat.m_dMarginRate;
					}
				}
				if (m_Order[0].m_lVolume != 0)
				{
					if (m_Deal[0].m_dMarginRate == 0)
					{
						m_Deal[0].m_dMarginRate = m_Order[0].m_dMarginRate;
					}
					double vr = 1.0 / ULL2DBL(m_Order[0].m_lVolume + m_Deal[0].m_lVolume);
					double orderAmount = ULL2DBL(m_Order[0].m_lVolume) * m_Order[0].m_dLots;
					double dealAmount = ULL2DBL(m_Deal[0].m_lVolume) * m_Deal[0].m_dLots;
					m_Deal[0].m_dLots = (orderAmount + dealAmount) * vr;
					orderAmount = ULL2DBL(m_Order[0].m_lVolume) * m_Order[0].m_dVolume;
					dealAmount = ULL2DBL(m_Deal[0].m_lVolume) * m_Deal[0].m_dVolume;
					m_Deal[0].m_dVolume = (orderAmount + dealAmount) * vr;
					m_Deal[0].m_lVolume += m_Order[0].m_lVolume;
					m_Deal[0].m_lOrderVolume = m_Order[0].m_lVolume;
				}
				if (m_Order[1].m_lVolume != 0)
				{
					if (m_Deal[1].m_dMarginRate == 0)
					{
						m_Deal[1].m_dMarginRate = m_Order[1].m_dMarginRate;
					}
					double vr = 1.0 / ULL2DBL(m_Order[1].m_lVolume + m_Deal[1].m_lVolume);
					double orderAmount = ULL2DBL(m_Order[1].m_lVolume) * m_Order[1].m_dLots;
					double dealAmount = ULL2DBL(m_Deal[1].m_lVolume) * m_Deal[1].m_dLots;
					m_Deal[1].m_dLots = (orderAmount + dealAmount) * vr;
					orderAmount = ULL2DBL(m_Order[1].m_lVolume) * m_Order[1].m_dVolume;
					dealAmount = ULL2DBL(m_Deal[1].m_lVolume) * m_Deal[1].m_dVolume;
					m_Deal[1].m_dVolume = (orderAmount + dealAmount) * vr;
					m_Deal[1].m_lVolume += m_Order[1].m_lVolume;
					m_Deal[1].m_lOrderVolume = m_Order[1].m_lVolume;
				}
				double margin = dealMargin;
				if (m_Order[0].m_lVolume != 0 || m_Order[1].m_lVolume != 0)
				{
					dealMargin = CalculateDealMargin();
					margin = Math.Max(margin, dealMargin);
				}
				m_dTradeMargin = margin + buyMargin + sellMargin;
			}
			trdInfo.m_dMargin = RoundAdd(trdInfo.m_dMargin, m_dTradeMargin, m_SymInfo.Digits);
			return m_dTradeMargin;
		}


		public double CalculateDealMargin()
		{
			double margin = 0;
			if (m_Deal[0].m_lVolume > m_Deal[1].m_lVolume)
			{
				ulong tv = m_Deal[0].m_lVolume - m_Deal[1].m_lVolume;
				if (m_dInitialMargin == 0 || m_dMaintenanceMargin == 0 || (m_dInitialMargin == m_dMaintenanceMargin) || m_Deal[0].m_lOrderVolume == 0)
				{
					margin = CalcHedMargin(m_pTradesInfo, tv, false, false, m_Deal[0].m_dLots);
				}
				else
				{
					ulong dv = tv;
					if (dv > m_Deal[0].m_lOrderVolume)
					{
						dv -= m_Deal[0].m_lOrderVolume;
					}
					tv -= dv;
					margin = CalcHedMargin(m_pTradesInfo, dv, true, false, m_Deal[0].m_dLots);
					if (tv != 0)
					{
						margin += CalcHedMargin(m_pTradesInfo, tv, false, false, m_Deal[0].m_dLots);
					}
				}
				margin *= m_Deal[0].m_dVolume * m_Deal[0].m_dMarginRate;
			}
			if (m_Deal[0].m_lVolume < m_Deal[1].m_lVolume)
			{
				ulong tv = m_Deal[1].m_lVolume - m_Deal[0].m_lVolume;
				if (m_dInitialMargin == 0 || m_dMaintenanceMargin == 0 || (m_dInitialMargin == m_dMaintenanceMargin) || m_Deal[1].m_lOrderVolume == 0)
				{
					margin = CalcHedMargin(m_pTradesInfo, tv, false, false, m_Deal[1].m_dLots);
				}
				else
				{
					ulong dv = tv;
					if (dv > m_Deal[1].m_lOrderVolume)
					{
						dv -= m_Deal[1].m_lOrderVolume;
					}
					tv -= dv;
					margin = CalcHedMargin(m_pTradesInfo, dv, true, false, m_Deal[1].m_dLots);
					if (tv != 0)
					{
						margin += CalcHedMargin(m_pTradesInfo, tv, false, false, m_Deal[1].m_dLots);
					}
				}
				margin *= m_Deal[1].m_dVolume * m_Deal[1].m_dMarginRate;
			}
			ulong volume = Math.Min(m_Deal[0].m_lVolume, m_Deal[1].m_lVolume);
			if (volume == 0)
			{
				return margin;
			}
			double vr = 1.0 / ULL2DBL(m_Deal[0].m_lVolume + m_Deal[1].m_lVolume);
			double buyVolume = ULL2DBL(m_Deal[0].m_lVolume);
			double sellVolume = ULL2DBL(m_Deal[1].m_lVolume);
			double lots = ((m_Deal[0].m_dLots * buyVolume) + (m_Deal[1].m_dLots * sellVolume)) * vr;
			double amount = ((m_Deal[0].m_dVolume * buyVolume) + (m_Deal[1].m_dVolume * sellVolume)) * vr;
			double rate = (m_Deal[0].m_dMarginRate + m_Deal[1].m_dMarginRate) * 0.5;
			return margin + CalcHedMargin(m_pTradesInfo, volume, false, true, lots) * amount * rate;
		}

		public double CalcHedMargin(TradesInfo trdInfo, ulong volume, bool bInitialMargin, bool bPrice, double lots)
		{
			double price = AsLots(volume, m_dContractSize);
			double amount = AsAmounts(volume);
			double initialMargin = m_dInitialMargin;
			if (!bInitialMargin && m_dMaintenanceMargin != 0)
			{
				initialMargin = m_dMaintenanceMargin;
			}
			if (bPrice)
			{
				if ((int)initialMargin != 0)
				{
					initialMargin = s160;
				}
				price = AsLots(volume, s160);
			}
			double margin = 0;
			double leveage = IntToDouble(trdInfo.m_nLeverage);
			switch (m_CalcMode)
			{
				case CalculationMode.Forex:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin / leveage;
					}
					else
					{
						margin = price / leveage;
					}
					break;
				case CalculationMode.Futures:
				case CalculationMode.ExchangeFutures:
				case CalculationMode.FORTSFutures:
				case CalculationMode.ExchangeMarginOption:
					margin = amount * initialMargin;
					break;
				case CalculationMode.CFD:
				case CalculationMode.ExchangeStocks:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin;
					}
					else
					{
						margin = price * lots;
					}
					break;
				case CalculationMode.CFDIndex:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin;
					}
					else if (m_dTickSize != 0)
					{
						margin = price * lots / m_dTickSize * m_dTickValue;
					}
					break;
				case CalculationMode.CFDLeverage:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin / leveage;
					}
					else
					{
						margin = price * lots / leveage;
					}
					break;
				case CalculationMode.CalcMode5:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin;
					}
					else
					{
						margin = price;
					}
					break;
				case CalculationMode.ExchangeBounds:
					if (initialMargin > 0)
					{
						margin = amount * initialMargin;
					}
					else
					{
						margin = Math.Round(price * lots * m_dFaceValue * 0.01, sE8);
					}
					break;
				case CalculationMode.Collateral:
					break;
			}
			return margin;
		}

		async Task<double> GetBidRate(string pCurrency1, string pCurrency2, double price = 0)
		{
			string cur = pCurrency1 + pCurrency2;
			if (Api.Symbols.Exist(cur))
			{
				if (price != 0 && ((m_SymInfo.CalcMode == CalculationMode.Forex) || (m_SymInfo.CalcMode == CalculationMode.CalcMode5)) && string.Compare(cur, m_SymInfo.Currency) != 0)
					return price;
				if (await Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
					return Double.NaN;
				return (await Api.GetQuoteInternal(cur, 0)).Bid;
			}
			cur = pCurrency2 + pCurrency1;
			if (Api.Symbols.Exist(cur))
			{
				if (price != 0 && ((m_SymInfo.CalcMode == CalculationMode.Forex) || (m_SymInfo.CalcMode == CalculationMode.CalcMode5)) && string.Compare(cur, m_SymInfo.Currency) != 0)
					return 1 / price;
				if (await Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
					return Double.NaN;
				return 1 / (await Api.GetQuoteInternal(cur, 0)).Ask;
			}
			return 0;
		}
		public async Task<double> GetBidProfitRate(string pCurrency1, string pCurrency2, double price = 0)
		{
			if (string.IsNullOrWhiteSpace(pCurrency1))
				throw new Exception("pCurrency1 is null or empty");
			if (string.IsNullOrWhiteSpace(pCurrency2))
				throw new Exception("pCurrency2 is null or empty");
			if (string.Compare(pCurrency1, pCurrency2) != 0 || IsRubleCurrency(pCurrency1, pCurrency2))
			{
				return 1.0;
			}
			double rate = await GetBidRate(pCurrency1, pCurrency2, price);
			if (Double.IsNaN(rate))
				return 0;
			if ((int)rate != 0)
			{
				return rate;
			}
			double toUSD = await GetBidRate(pCurrency1, "USD", price);
			if (Double.IsNaN(rate))
				return 0;
			if (toUSD == 0)
			{
				return 0;
			}
			double fromUsd = await GetBidRate("USD", pCurrency2, price);
			if (Double.IsNaN(rate))
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

		protected async Task<double> GetAskRate(string pCurrency1, string pCurrency2, double price = 0)
		{
			string cur = pCurrency1 + pCurrency2;
			if (Api.Symbols.Exist(cur))
			{
				if (price != 0 && ((m_SymInfo.CalcMode == CalculationMode.Forex) || (m_SymInfo.CalcMode == CalculationMode.CalcMode5)) && string.Compare(cur, m_SymInfo.Currency) != 0)
					return price;
				if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
					return Double.NaN;
				return (await Api.GetQuoteInternal(cur, 0)).Ask;
			}
			cur = pCurrency2 + pCurrency1;
			if (Api.Symbols.Exist(cur))
			{
				if (price != 0 && ((m_SymInfo.CalcMode == CalculationMode.Forex) || (m_SymInfo.CalcMode == CalculationMode.CalcMode5)) && string.Compare(cur, m_SymInfo.Currency) != 0)
					return 1 / price;
				if (Api.GetQuoteInternal(cur, Api.GetQuoteTimeoutMs) == null)
					return Double.NaN;
				return 1 / (await Api.GetQuoteInternal(cur, 0)).Bid;
			}
			return 0;
		}

		public async Task<double> GetAskProfitRate(string pCurrency1, string pCurrency2, double price = 0)
		{
			if (string.IsNullOrWhiteSpace(pCurrency1))
				throw new Exception("pCurrency1 is null or empty");
			if (string.IsNullOrWhiteSpace(pCurrency2))
				throw new Exception("pCurrency2 is null or empty");
			if (string.Compare(pCurrency1, pCurrency2) != 0 || IsRubleCurrency(pCurrency1, pCurrency2))
			{
				return 1.0;
			}
			double rate = await GetAskRate(pCurrency1, pCurrency2, price);
			if (Double.IsNaN(rate))
				return 0;
			if ((int)rate != 0)
			{
				return rate;
			}
			double toUSD = await GetAskRate(pCurrency1, "USD", price);
			if (Double.IsNaN(rate))
				return 0;
			if (toUSD == 0)
			{
				return 0;
			}
			double fromUSD = await GetAskRate("USD", pCurrency2, price);
			if (Double.IsNaN(rate))
				return 0;
			if (fromUSD == 0)
			{
				return 0;
			}
			return toUSD * fromUSD;
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

		protected async Task<double> GetBid(string symbol)
		{
			if (Api.GetQuoteInternal(symbol, Api.GetQuoteTimeoutMs) == null)
				return 0;
			return (await Api.GetQuoteInternal(symbol, 0)).Bid;
		}

		protected async Task<double> GetAsk(string symbol)
		{
			if (Api.GetQuoteInternal(symbol, Api.GetQuoteTimeoutMs) == null)
				return 0;
			return (await Api.GetQuoteInternal(symbol, 0)).Ask;
		}

		double GetMarginRate(OrderType type, bool init, string symbol)
		{
			var initMarginRate = Api.Symbols.GetGroup(symbol).InitMarginRate;
			var maintainceMarginRate = Api.Symbols.GetGroup(symbol).MntnMarginRate;
			double im = 0;
			double mm = 0;
			switch (type)
			{
				case OrderType.Buy:
					im = initMarginRate[0];
					mm = maintainceMarginRate[0];
					break;
				case OrderType.Sell:
					im = initMarginRate[1];
					mm = maintainceMarginRate[1];
					break;
				case OrderType.BuyLimit:
					im = initMarginRate[2];
					mm = maintainceMarginRate[2];
					break;
				case OrderType.SellLimit:
					im = initMarginRate[3];
					mm = maintainceMarginRate[3];
					break;
				case OrderType.BuyStop:
					im = initMarginRate[4];
					mm = maintainceMarginRate[4];
					break;
				case OrderType.SellStop:
					im = initMarginRate[5];
					mm = maintainceMarginRate[5];
					break;
				case OrderType.BuyStopLimit:
					im = initMarginRate[6];
					mm = maintainceMarginRate[6];
					break;
				case OrderType.SellStopLimit:
					im = initMarginRate[7];
					mm = maintainceMarginRate[7];
					break;
			}
			return init ? im : mm;
		}

		double RoundAdd(double value1, double value2, int digits)
		{
			return Math.Round(value1 + value2, digits);
		}

		double ULL2DBL(ulong value)
		{
			return (double)value;
		}

		double AsLots(ulong value, double lots)
		{
			return Math.Round(ULL2DBL(value) * 1.0e-8 * lots, 8);
		}

		double AsAmounts(ulong value)
		{
			return Math.Round(ULL2DBL(value) * 1.0e-8, 8);
		}

		double IntToDouble(int value)
		{
			double _DP2to32 = 4.294967296e9;
			double res = (double)value;
			if (value < 0)
				res += _DP2to32;
			return res;
		}

	}
}
