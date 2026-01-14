using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	public class MarketWatch
	{
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol;
        /// <summary>
        /// High
        /// </summary>
        public double High;
        /// <summary>
		/// Low
		/// </summary>
		public double Low;
        /// <summary>
		/// Open price
		/// </summary>
		public double OpenPrice;
        /// <summary>
        /// Close price
        /// </summary>
        public double ClosePrice;
        /// <summary>
		/// Daily change in percents
		/// </summary>
		public double DailyChange;
		/// <summary>
		/// Bid
		/// </summary>
		public double Bid;
		/// <summary>
		/// Ask
		/// </summary>
		public double Ask;
		/// <summary>
		/// Spread
		/// </summary>
		public int Spread;
		/// <summary>
		/// Volume
		/// </summary>
        public ulong Volume;
    }


    class SymbolSummaryInternal
	{
		public string Symbol;
		public int Id;
		public ulong UpdateMask;
		public long Time;
		//private int s14;
		public long Bid;
		public long BidHigh;
		public long BidLow;
		public long Ask;
		public long AskHigh;
		public long AskLow;
		public long Last;
		public long LastHigh;
		public long LastLow;
		public ulong Volume;
		public ulong VolumeHigh;
		public ulong VolumeLow;
		public long Deals;
		public ulong DealsVolume;
		public long Turnover;
		public long OpenInterest;
		public ulong BuyOrders;
		public ulong BuyVolume;
		public ulong SellOrders;
		public ulong SellVolume;
		public long OpenPrice;
		public long ClosePrice;
		public long AverageWeightPrice;
		public long PriceChange;
		public long PriceVolatility;
		public long PriceTheoretical;
		public long TimeMs;
		public long PriceDelta;
		public long PriceTheta;
		public long PriceGamma;
		public long PriceVega;
		public long PriceRho;
		public long PriceOmega;
		public long PriceSensitivity;
	}
}
