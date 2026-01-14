using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x4CC, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Symbol group information
	/// </summary>
	public class SymGroup  : FromBufReader, IEqualityComparer<SymGroup>, IEqualityComparer
    {
		/*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private byte[] s0;
		/*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string GroupName;
		/*[FieldOffset(264)]*/ public int DeviationRate;
		/*[FieldOffset(268)]*/ public int RoundRate;
		/*[FieldOffset(272)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] s110;
		/*[FieldOffset(304)]*/ public TradeMode TradeMode;
		/*[FieldOffset(308)]*/ public int SL;
		/*[FieldOffset(312)]*/ public int TP;
		/*[FieldOffset(316)]*/ public ExecutionType TradeType;
		/*[FieldOffset(320)]*/ public FillingFlags FillPolicy;
		/*[FieldOffset(324)]*/ public ExpirationFlags Expiration;
		/*[FieldOffset(328)]*/ public int OrderFlags;
		/*[FieldOffset(332)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 52)]*/ private byte[] s14C;
		/*[FieldOffset(384)]*/ private int s180;
		/*[FieldOffset(388)]*/ public int PriceTimeout;
		/*[FieldOffset(392)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] s188;
		/*[FieldOffset(424)]*/ private int s1A8;
		/*[FieldOffset(428)]*/ public int RequoteTimeout;
		/*[FieldOffset(432)]*/ private int s1B0;
		/*[FieldOffset(436)]*/ private int s1B4;
		/*[FieldOffset(440)]*/ private int s1B8;
		/*[FieldOffset(444)]*/ public uint RequestLots;
		/*[FieldOffset(448)]*/ private int s1C4;
		/*[FieldOffset(452)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]*/ private byte[] s1C8;
		/*[FieldOffset(476)]*/ private int s1E0;
		/*[FieldOffset(480)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] s1E4;
		/*[FieldOffset(608)]*/ private int s264;
		/*[FieldOffset(612)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] s268;
		/*[FieldOffset(740)]*/ public int tmp1;
		public double MinLots { get { return (double) MinVolume / 100000000; } }
		/*[FieldOffset(744)]*/ public ulong MinVolume;
		/*[FieldOffset(748)]*/
		public double MaxLots { get { return (double)MaxVolume / 100000000; } }
		/*[FieldOffset(752)]*/
		public ulong MaxVolume;
		/*[FieldOffset(756)]*/
		public double LotsStep { get { return (double)VolumeStep / 100000000; } }
		/*[FieldOffset(760)]*/
		public long VolumeStep;
		/*[FieldOffset(768)]*/ private long s300;
		/*[FieldOffset(776)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 56)]*/ private byte[] s308;
		/*[FieldOffset(832)]*/ internal int s340;
		/*[FieldOffset(836)]*/ public double InitialMargin;
		/*[FieldOffset(844)]*/ public double MaintenanceMargin;
		/*[FieldOffset(852)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ public double[] InitMarginRate;
		/*[FieldOffset(916)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ public double[] MntnMarginRate;
		/*[FieldOffset(980)]*/ internal double s3D4;
		/*[FieldOffset(988)]*/ public double HedgedMargin;
		/*[FieldOffset(996)]*/ internal double s3E4; 
		/*[FieldOffset(1004)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 40)]*/ private byte[] s3EC;
		/*[FieldOffset(1044)]*/ public SwapType SwapType;
		/*[FieldOffset(1048)]*/ public double SwapLong;
		/*[FieldOffset(1056)]*/ public double SwapShort;
		/*[FieldOffset(1064)]*/ public V3DaysSwap ThreeDaysSwap;
		/*[FieldOffset(1068)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] s42C;
		/*[FieldOffset(1100)]*/ private int s44C;
		/*[FieldOffset(1104)]*/ private int s450;
		/*[FieldOffset(1108)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 120)]*/ private byte[] s454;
		public double[] SwapRates = new double[7] { 0.0, 1.0 , 1.0 , 3.0, 1.0, 1.0, 0.0 };
        internal static int Size = 0x4CC;

        internal override object ReadFromBuf(InBuf buf)
		{
			var startInd = buf.CurrentIndex;
			var endInd = buf.CurrentIndex + 1228;
			var st = new SymGroup ();
			//st.s0 = new byte[8];
			//for (int i = 0; i < 8; i++)
			//	st.s0[i] = buf.Byte();
			buf.Bytes(8);
			st.GroupName = GetString(buf.Bytes(256));
			st.DeviationRate = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.RoundRate = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s110 = new byte[32];
			//for (int i = 0; i < 32; i++)
			//	st.s110[i] = buf.Byte();
			buf.Bytes(32);
			st.TradeMode = (TradeMode)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.SL = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TP = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TradeType = (ExecutionType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.FillPolicy = (FillingFlags)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Expiration = (ExpirationFlags)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.OrderFlags = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s14C = new byte[52];
			//for (int i = 0; i < 52; i++)
			//	st.s14C[i] = buf.Byte();
			buf.Bytes(52);
			st.s180 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.PriceTimeout = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s188 = new byte[32];
			//for (int i = 0; i < 32; i++)
			//	st.s188[i] = buf.Byte();
			buf.Bytes(32);
			st.s1A8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.RequoteTimeout = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1B0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1B4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1B8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.RequestLots = BitConverter.ToUInt32(buf.Bytes(4), 0);
			st.s1C4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s1C8 = new byte[24];
			//for (int i = 0; i < 24; i++)
			//	st.s1C8[i] = buf.Byte();
			buf.Bytes(24);
			st.s1E0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s1E4 = new byte[128];
			//for (int i = 0; i < 128; i++)
			//	st.s1E4[i] = buf.Byte();
			buf.Bytes(128);
			st.s264 = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s268 = new byte[128];
			//for (int i = 0; i < 128; i++)
			//	st.s268[i] = buf.Byte();
			buf.Bytes(128);
			st.tmp1 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.MinVolume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.MaxVolume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.VolumeStep = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s300 = BitConverter.ToInt64(buf.Bytes(8), 0);
			//st.s308 = new byte[56];
			//for (int i = 0; i < 56; i++)
			//	st.s308[i] = buf.Byte();
			buf.Bytes(56);
			st.s340 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.InitialMargin = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.MaintenanceMargin = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.InitMarginRate = new double[8];
			for (int i = 0; i < 8; i++)
				st.InitMarginRate[i] = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.MntnMarginRate = new double[8];
			for (int i = 0; i < 8; i++)
				st.MntnMarginRate[i] = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s3D4 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.HedgedMargin = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s3E4 = BitConverter.ToDouble(buf.Bytes(8), 0);
			//st.s3EC = new byte[40];
			//for (int i = 0; i < 40; i++)
			//	st.s3EC[i] = buf.Byte();
			buf.Bytes(40);
			st.SwapType = (SwapType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.SwapLong = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.SwapShort = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.ThreeDaysSwap = (V3DaysSwap)BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s42C = new byte[32];
			//for (int i = 0; i < 32; i++)
			//	st.s42C[i] = buf.Byte();
			buf.Bytes(32);
			st.s44C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s450 = BitConverter.ToInt32(buf.Bytes(4), 0);
			//st.s454 = new byte[120];
			//for (int i = 0; i < 120; i++)
			//	st.s454[i] = buf.Byte();
			for (int i = 0; i < 7; i++)
				SwapRates[i] = BitConverter.ToDouble(buf.Bytes(8), 0);
			buf.Bytes(64);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}

		public void CopyValues(SymGroup grp)
		{
			if (grp.DeviationRate != int.MaxValue)
				DeviationRate = grp.DeviationRate;
			if (grp.RoundRate != int.MaxValue)
				RoundRate = grp.RoundRate;
			if ((int)grp.TradeMode != -1)
				TradeMode = grp.TradeMode;
			if (grp.SL != int.MaxValue)
				SL = grp.SL;
			if (grp.TP != int.MaxValue)
				TP = grp.TP;
			if ((int)grp.TradeType != -1)
				TradeType = grp.TradeType;
			if ((int)grp.FillPolicy != -1)
				FillPolicy = grp.FillPolicy;
			if ((int)grp.Expiration != -1)
				Expiration = grp.Expiration;
			if (grp.OrderFlags != -1)
				OrderFlags = grp.OrderFlags;
			if (grp.s180 != -1)
				s180 = grp.s180;
			if (grp.PriceTimeout != -1)
				PriceTimeout = grp.PriceTimeout;
			if (grp.s1A8 != -1)
				s1A8 = grp.s1A8;
			if (grp.RequoteTimeout != -1)
				RequoteTimeout = grp.RequoteTimeout;
			if (grp.s1B0 != -1)
				s1B0 = grp.s1B0;
			if (grp.s1B4 != -1)
				s1B4 = grp.s1B4;
			if (grp.RequestLots != uint.MaxValue)
				RequestLots = grp.RequestLots;
			if (grp.s1C4 != -1)
				s1C4 = grp.s1C4;
			if (grp.MinVolume != ulong.MaxValue)
				MinVolume = grp.MinVolume;
			if (grp.MaxVolume != ulong.MaxValue)
				MaxVolume = grp.MaxVolume;
			if (grp.VolumeStep != -1)
				VolumeStep = grp.VolumeStep;
			if (grp.s300 != -1)
				s300 = grp.s300;
			if (grp.s340 != -1)
				s340 = grp.s340;
			if (grp.InitialMargin != double.MaxValue)
				InitialMargin = grp.InitialMargin;
			if (grp.MaintenanceMargin != double.MaxValue)
				MaintenanceMargin = grp.MaintenanceMargin;
			if (grp.InitMarginRate[0] != double.MaxValue)
				InitMarginRate[0] = grp.InitMarginRate[0];
			if (grp.InitMarginRate[1] != double.MaxValue)
				InitMarginRate[1] = grp.InitMarginRate[1];
			if (grp.InitMarginRate[2] != double.MaxValue)
				InitMarginRate[2] = grp.InitMarginRate[2];
			if (grp.InitMarginRate[3] != double.MaxValue)
				InitMarginRate[3] = grp.InitMarginRate[3];
			if (grp.InitMarginRate[4] != double.MaxValue)
				InitMarginRate[4] = grp.InitMarginRate[4];
			if (grp.InitMarginRate[5] != double.MaxValue)
				InitMarginRate[5] = grp.InitMarginRate[5];
			if (grp.InitMarginRate[6] != double.MaxValue)
				InitMarginRate[6] = grp.InitMarginRate[6];
			if (grp.InitMarginRate[7] != double.MaxValue)
				InitMarginRate[7] = grp.InitMarginRate[7];
			if (grp.MntnMarginRate[0] != double.MaxValue)
				MntnMarginRate[0] = grp.MntnMarginRate[0];
			if (grp.MntnMarginRate[1] != double.MaxValue)
				MntnMarginRate[1] = grp.MntnMarginRate[1];
			if (grp.MntnMarginRate[2] != double.MaxValue)
				MntnMarginRate[2] = grp.MntnMarginRate[2];
			if (grp.MntnMarginRate[3] != double.MaxValue)
				MntnMarginRate[3] = grp.MntnMarginRate[3];
			if (grp.MntnMarginRate[4] != double.MaxValue)
				MntnMarginRate[4] = grp.MntnMarginRate[4];
			if (grp.MntnMarginRate[5] != double.MaxValue)
				MntnMarginRate[5] = grp.MntnMarginRate[5];
			if (grp.MntnMarginRate[6] != double.MaxValue)
				MntnMarginRate[6] = grp.MntnMarginRate[6];
			if (grp.MntnMarginRate[7] != double.MaxValue)
				MntnMarginRate[7] = grp.MntnMarginRate[7];
			if (grp.s3D4 != double.MaxValue)
				s3D4 = grp.s3D4;
			if (grp.HedgedMargin != double.MaxValue)
				HedgedMargin = grp.HedgedMargin;
			if (grp.s3E4 != double.MaxValue)
				s3E4 = grp.s3E4;
			if ((int)grp.SwapType != -1)
				SwapType = grp.SwapType;
			if (grp.SwapLong != double.MaxValue)
				SwapLong = grp.SwapLong;
			if (grp.SwapShort != double.MaxValue)
				SwapShort = grp.SwapShort;
			if ((int)grp.ThreeDaysSwap != int.MaxValue)
				ThreeDaysSwap = grp.ThreeDaysSwap;
			for (int i = 0; i < 7; i++)
				if (grp.SwapRates[i] != double.MaxValue)
					SwapRates[i] = grp.SwapRates[i];
			s44C = grp.s44C;
			s450 = grp.s450;
		}

        public static bool AreEqual(SymGroup sg1, SymGroup sg2)
        {
			// Compare all public fields and properties 
			return sg1.GroupName == sg2.GroupName &&
				   sg1.DeviationRate == sg2.DeviationRate &&
				   sg1.RoundRate == sg2.RoundRate &&
				   sg1.TradeMode == sg2.TradeMode &&
				   sg1.SL == sg2.SL &&
				   sg1.TP == sg2.TP &&
				   sg1.TradeType == sg2.TradeType &&
				   sg1.FillPolicy == sg2.FillPolicy &&
				   sg1.Expiration == sg2.Expiration &&
				   sg1.OrderFlags == sg2.OrderFlags &&
				   sg1.PriceTimeout == sg2.PriceTimeout &&
				   sg1.RequoteTimeout == sg2.RequoteTimeout &&
				   sg1.RequestLots == sg2.RequestLots &&
				   sg1.tmp1 == sg2.tmp1 &&
				   Math.Abs(sg1.MinLots - sg2.MinLots) < 1e-8 && // Comparing doubles with tolerance
				   Math.Abs(sg1.MaxLots - sg2.MaxLots) < 1e-8 &&
				   Math.Abs(sg1.LotsStep - sg2.LotsStep) < 1e-8 &&
				   sg1.MinVolume == sg2.MinVolume &&
				   sg1.MaxVolume == sg2.MaxVolume &&
				   sg1.VolumeStep == sg2.VolumeStep &&
				   sg1.InitialMargin == sg2.InitialMargin &&
				   sg1.MaintenanceMargin == sg2.MaintenanceMargin &&
				   Math.Abs(sg1.HedgedMargin - sg2.HedgedMargin) < 1e-8 &&
				   sg1.SwapType == sg2.SwapType &&
				   Math.Abs(sg1.SwapLong - sg2.SwapLong) < 1e-8 &&
				   Math.Abs(sg1.SwapShort - sg2.SwapShort) < 1e-8 &&
				   sg1.ThreeDaysSwap == sg2.ThreeDaysSwap &&
				   AreDoublesEqual(sg1.InitMarginRate, sg2.InitMarginRate) &&
				   AreDoublesEqual(sg1.MntnMarginRate, sg2.MntnMarginRate) &&
				   AreDoublesEqual(sg1.SwapRates, sg2.SwapRates);
				   //&& AreBytesEqual(sg1.SymbolsHash, sg2.SymbolsHash) &&
                   //AreBytesEqual(sg1.SpreadsHash, sg2.SpreadsHash);
        }

        private static bool AreDoublesEqual(double[] arr1, double[] arr2)
        {
            if (arr1 == null || arr2 == null)
            {
                return arr1 == arr2; // Both should be null to be considered equal
            }

            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            // Allow for slight discrepancies in double precision
            for (int i = 0; i < arr1.Length; i++)
            {
                if (Math.Abs(arr1[i] - arr2[i]) >= 1e-8)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AreBytesEqual(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
            {
                return arr1 == arr2; // Both should be null to be considered equal
            }
            return arr1.SequenceEqual(arr2);
        }

        public new bool Equals(object x, object y)
        {
            return AreEqual((SymGroup)x, (SymGroup)y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public new bool Equals(SymGroup x, SymGroup y)
        {
            return AreEqual((SymGroup)x, (SymGroup)y);
        }

        public int GetHashCode(SymGroup obj)
        {
            return obj.GetHashCode();
        }
    }
}
