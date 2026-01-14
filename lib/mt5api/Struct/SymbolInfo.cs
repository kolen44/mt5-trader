using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x6A0, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Symbol details
    /// </summary>
    public class SymbolInfo : FromBufReader, IEqualityComparer<SymbolInfo>, IEqualityComparer
    {
        /// <summary>
        /// Update time
        /// </summary>
        /*[FieldOffset(0)]*/ public long UpdateTime;
        /// <summary>
        /// Symbol currency
        /// </summary>
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Currency;
        /// <summary>
        /// Symbol ISIN
        /// </summary>
        /*[FieldOffset(72)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string ISIN;
        /// <summary>
        /// Description
        /// </summary>
        /*[FieldOffset(104)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ public string Description;
        /*[FieldOffset(232)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string sE8;
        /*[FieldOffset(360)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Basis;
        /*[FieldOffset(424)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ private string s1A8;
        /// <summary>
        /// Reference to site
        /// </summary>
        /*[FieldOffset(488)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/ public string RefToSite;
        /*[FieldOffset(1000)]*/ public int Custom;
        /*[FieldOffset(1004)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 28)]*/ private byte[] s3EC;
        /*[FieldOffset(1032)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ private string s408;
        /// <summary>
        /// Currency for profit
        /// </summary>
        /*[FieldOffset(1064)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string ProfitCurrency;
        /// <summary>
        /// Currency for margin
        /// </summary>
        /*[FieldOffset(1096)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string MarginCurrency;
        /*[FieldOffset(1128)]*/ internal int s468;
        /*[FieldOffset(1132)]*/ public int Precision;
        /*[FieldOffset(1136)]*/ private int s470;
        /*[FieldOffset(1140)]*/ private int s474;
        /*[FieldOffset(1144)]*/ private int s478;
        /// <summary>
        /// Background color
        /// </summary>
        /*[FieldOffset(1148)]*/ public int BkgndColor;
        /*[FieldOffset(1152)]*/ private int s480;
        /*[FieldOffset(1156)]*/ private int s484;
        /*[FieldOffset(1160)]*/ private int s488;
        /*[FieldOffset(1164)]*/ private int s48C;
        /// <summary>
        /// Significant digits
        /// </summary>
        /*[FieldOffset(1168)]*/ public int Digits;
        /// <summary>
        /// Symbol points
        /// </summary>
        /*[FieldOffset(1172)]*/ public double Points;
        /// <summary>
        /// Symbol limit points
        /// </summary>
        /*[FieldOffset(1180)]*/ public double LimitPoints;
        /// <summary>
        /// Symbol id
        /// </summary>
        /*[FieldOffset(1188)]*/ public int Id;
        /*[FieldOffset(1192)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] s4A8;
        /*[FieldOffset(1224)]*/ private long s4C8;
        /// <summary>
        /// Depth of market
        /// </summary>
        /*[FieldOffset(1232)]*/ public int DepthOfMarket;
        /*[FieldOffset(1236)]*/ private int s4D4;
        /*[FieldOffset(1240)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 36)]*/ private byte[] s4D8;
        /*[FieldOffset(1276)]*/ private int s4FC;
        /*[FieldOffset(1280)]*/ private int s500;
        /*[FieldOffset(1284)]*/ private int s504;
        /*[FieldOffset(1288)]*/ private int s508;
        /*[FieldOffset(1292)]*/ private int s50C;
        /*[FieldOffset(1296)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] s510;
        /*[FieldOffset(1360)]*/ internal long s550;
        /// <summary>
        /// Spread
        /// </summary>
        /*[FieldOffset(1368)]*/ public int Spread;
        /*[FieldOffset(1372)]*/ private int s55C;
        /// <summary>
        /// Tick value
        /// </summary>
        /*[FieldOffset(1376)]*/ public double TickValue;
        /// <summary>
        /// Tick size
        /// </summary>
        /*[FieldOffset(1384)]*/ public double TickSize;
        /// <summary>
        /// Contract size
        /// </summary>
        /*[FieldOffset(1392)]*/ public double ContractSize;
        /// <summary>
        /// Good till canceled mode
        /// </summary>
        /*[FieldOffset(1400)]*/ public GTCMode GTCMode;
        /// <summary>
        /// Calculation mode
        /// </summary>
        /*[FieldOffset(1404)]*/ public CalculationMode CalcMode;
        /*[FieldOffset(1408)]*/ private int s580;
        /*[FieldOffset(1412)]*/ private int s584;
        /// <summary>
        /// Settlement price
        /// </summary>
        /*[FieldOffset(1416)]*/ public double SettlementPrice;
        /// <summary>
        /// Lower limit
        /// </summary>
        /*[FieldOffset(1424)]*/ public double LowerLimit;
        /// <summary>
        /// Upper limit
        /// </summary>
        /*[FieldOffset(1432)]*/ public double UpperLimit;
        /*[FieldOffset(1440)]*/ private double s5A0;
        /*[FieldOffset(1448)]*/ private int s5A8;
        /// <summary>
        /// Face value
        /// </summary>
        /*[FieldOffset(1452)]*/ public double FaceValue;
        /// <summary>
        /// Accuired interest
        /// </summary>
        /*[FieldOffset(1460)]*/ public double AccruedInterest;
        /*[FieldOffset(1468)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12)]*/ private byte[] s5BC;
        /*[FieldOffset(1480)]*/ private long s5C8;
        /// <summary>
        /// First trade time
        /// </summary>
        /*[FieldOffset(1488)]*/ public long FirstTradeTime;
        /// <summary>
        /// Last trade time
        /// </summary>
        /*[FieldOffset(1496)]*/ public long LastTradeTime;
        /*[FieldOffset(1504)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] s5E0;
        /*[FieldOffset(1568)]*/ private int s620;
        /*[FieldOffset(1572)]*/ private int s624;
        /*[FieldOffset(1576)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 120)]*/ private byte[] s628;

		public double bid_tickvalue;
		public double ask_tickvalue;
        private string s68;
        private string _sE8;
        internal static int Size = 0x6A0;

        internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 1696;
            if (buf.SymBuild > 2097)
                endInd += 256;
            var st = new SymbolInfo();
			st.UpdateTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Currency = GetString(buf.Bytes(64));
			st.ISIN = GetString(buf.Bytes(32));
            if (buf.SymBuild > 2097)
            {
                /*st.s68 =*/ GetString(buf.Bytes(128));
               /* st._sE8 =*/ GetString(buf.Bytes(128));
            }
            st.Description = GetString(buf.Bytes(128));
			/*st.sE8 =*/ GetString(buf.Bytes(128));
            st.Basis = GetString(buf.Bytes(64));
			st.s1A8 = GetString(buf.Bytes(64));
			st.RefToSite = GetString(buf.Bytes(512));
			st.Custom = BitConverter.ToInt32(buf.Bytes(4), 0);
            /*st.s3EC =*/ buf.Bytes(28);
			/*st.s408 =*/ GetString(buf.Bytes(32));
			st.ProfitCurrency = GetString(buf.Bytes(32));
			st.MarginCurrency = GetString(buf.Bytes(32));
			st.s468 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Precision = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s470 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s474 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s478 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.BkgndColor = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s480 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s484 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s488 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s48C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Digits = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Points = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.LimitPoints = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Id = BitConverter.ToInt32(buf.Bytes(4), 0);
            /*st.s4A8 =*/ buf.Bytes(32);
			st.s4C8 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.DepthOfMarket = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s4D4 = BitConverter.ToInt32(buf.Bytes(4), 0);
            /*st.s4D8 =*/ buf.Bytes(36);
			st.s4FC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s500 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s504 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s508 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s50C = BitConverter.ToInt32(buf.Bytes(4), 0);
            /*st.s510 =*/ buf.Bytes(64);
			st.s550 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Spread = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s55C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TickValue = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.TickSize = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.ContractSize = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.GTCMode = (GTCMode)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.CalcMode = (CalculationMode)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s580 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s584 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.SettlementPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.LowerLimit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.UpperLimit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s5A0 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s5A8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.FaceValue = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.AccruedInterest = BitConverter.ToDouble(buf.Bytes(8), 0);
            /*st.s5BC =*/ buf.Bytes(12);
			st.s5C8 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.FirstTradeTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.LastTradeTime = BitConverter.ToInt64(buf.Bytes(8), 0);
            /*st.s5E0 =*/ buf.Bytes(64);
			st.s620 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s624 = BitConverter.ToInt32(buf.Bytes(4), 0);
            /*st.s628 =*/ buf.Bytes(120);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex + " != "+endInd);
			return st;
		}

        public static bool AreEqual(SymbolInfo si1, SymbolInfo si2)
        {
            return si1.UpdateTime == si2.UpdateTime &&
                   si1.Currency == si2.Currency &&
                   si1.ISIN == si2.ISIN &&
                   si1.Description == si2.Description &&
                   si1.Basis == si2.Basis &&
                   si1.RefToSite == si2.RefToSite &&
                   si1.Custom == si2.Custom &&
                   si1.ProfitCurrency == si2.ProfitCurrency &&
                   si1.MarginCurrency == si2.MarginCurrency &&
                   si1.Precision == si2.Precision &&
                   si1.BkgndColor == si2.BkgndColor &&
                   si1.Digits == si2.Digits &&
                   Math.Abs(si1.Points - si2.Points) < 1e-8 &&
                   Math.Abs(si1.LimitPoints - si2.LimitPoints) < 1e-8 &&
                   si1.Id == si2.Id &&
                   si1.DepthOfMarket == si2.DepthOfMarket &&
                   si1.Spread == si2.Spread &&
                   Math.Abs(si1.TickValue - si2.TickValue) < 1e-8 &&
                   Math.Abs(si1.TickSize - si2.TickSize) < 1e-8 &&
                   Math.Abs(si1.ContractSize - si2.ContractSize) < 1e-8 &&
                   si1.GTCMode == si2.GTCMode &&
                   si1.CalcMode == si2.CalcMode &&
                   Math.Abs(si1.SettlementPrice - si2.SettlementPrice) < 1e-8 &&
                   Math.Abs(si1.LowerLimit - si2.LowerLimit) < 1e-8 &&
                   Math.Abs(si1.UpperLimit - si2.UpperLimit) < 1e-8 &&
                   Math.Abs(si1.FaceValue - si2.FaceValue) < 1e-8 &&
                   Math.Abs(si1.AccruedInterest - si2.AccruedInterest) < 1e-8 &&
                   si1.FirstTradeTime == si2.FirstTradeTime &&
                   si1.LastTradeTime == si2.LastTradeTime &&
                   AreByteArraysEqual(si1.s3EC, si2.s3EC) &&
                   AreByteArraysEqual(si1.s4A8, si2.s4A8) &&
                   AreByteArraysEqual(si1.s4D8, si2.s4D8) &&
                   AreByteArraysEqual(si1.s510, si2.s510) &&
                   AreByteArraysEqual(si1.s5BC, si2.s5BC) &&
                   AreByteArraysEqual(si1.s5E0, si2.s5E0) &&
                   AreByteArraysEqual(si1.s628, si2.s628);
        }

        private static bool AreByteArraysEqual(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
            {
                return arr1 == arr2; // Both should be null to be considered equal
            }

            return arr1.SequenceEqual(arr2);
        }

        public new bool Equals(object x, object y)
        {
            return AreEqual((SymbolInfo)x, (SymbolInfo)y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(SymbolInfo x, SymbolInfo y)
        {
            return AreEqual((SymbolInfo)x, (SymbolInfo)y);
        }

        public int GetHashCode(SymbolInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
