using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Size = 0x320, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Trade request
	/// </summary>
	public class TradeRequest : FromBufReader
	{
		// <summary>
		/// Request id
		/// </summary>
		/*[FieldOffset(0)]*/		public int RequestId;
		/*[FieldOffset(4)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/		private byte[] s4;
		/// <summary>
		/// Trade type
		/// </summary>
		/*[FieldOffset(68)]*/		public TradeType TradeType;
		/// <summary>
		/// Account login
		/// </summary>
		/*[FieldOffset(72)]*/		public ulong Login;
		/*[FieldOffset(80)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 72)]*/		private byte[] s50;
		/// <summary>
		/// Transfer login
		/// </summary>
		/*[FieldOffset(152)]*/	public ulong TransferLogin;
		/*[FieldOffset(160)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/		private string sA0;
		/// <summary>
		/// Symbol currency
		/// </summary>
		/*[FieldOffset(288)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/		public string Currency;
		/// <summary>
		/// Lots
		/// </summary>
		public double Lots { get { return (double)Volume / 100000000; } }
		/*[FieldOffset(352)]*/
		internal ulong Volume;
		
		/*[FieldOffset(360)]*/		private long s168;
		/// <summary>
		/// Significant digits
		/// </summary>
		/*[FieldOffset(368)]*/		public int Digits;
		/*[FieldOffset(372)]*/		internal long s174;
		/*[FieldOffset(380)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]*/		private byte[] s17C;
		/// <summary>
		/// Order ticket
		/// </summary>
		/*[FieldOffset(400)]*/		public long OrderTicket;
		/*[FieldOffset(408)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/		private byte[] s198;
		/*[FieldOffset(472)]*/		private long s1D8;
		/// <summary>
		/// Expiration time
		/// </summary>
		/*[FieldOffset(480)]*/		public long ExpirationTime;
		/// <summary>
		/// Order type
		/// </summary>
		/*[FieldOffset(488)]*/		public OrderType OrderType;
		/// <summary>
		/// Fill policy
		/// </summary>
		/*[FieldOffset(492)]*/		public FillPolicy FillPolicy;
		/// <summary>
		/// Expiration type
		/// </summary>
		/*[FieldOffset(496)]*/		public ExpirationType ExpirationType;
		/// <summary>
		/// Request flags
		/// </summary>
		/*[FieldOffset(500)]*/		public long Flags;
		/// <summary>
		/// Placed type
		/// </summary>
		/*[FieldOffset(508)]*/		public PlacedType PlacedType;
		/*[FieldOffset(512)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/		private byte[] s200;
		/// <summary>
		/// Price
		/// </summary>
		/*[FieldOffset(528)]*/		public double Price;
		/// <summary>
		/// Order price
		/// </summary>
		/*[FieldOffset(536)]*/		public double OrderPrice;
		/// <summary>
		/// Stop loss
		/// </summary>
		/*[FieldOffset(544)]*/		public double StopLoss;
		/// <summary>
		/// Take profit
		/// </summary>
		/*[FieldOffset(552)]*/		public double TakeProfit;
		/// <summary>
		/// Deviation
		/// </summary>
		/*[FieldOffset(560)]*/		public ulong Deviation;
		/*[FieldOffset(568)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/		private byte[] s238;
		/// <summary>
		/// Expert id
		/// </summary>
		/*[FieldOffset(600)]*/		public long ExpertId;
		/// <summary>
		/// Text comment
		/// </summary>
		/*[FieldOffset(608)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/		public string Comment;
		/// <summary>
		/// Deal ticket
		/// </summary>
		/*[FieldOffset(672)]*/		public long DealTicket;
		/// <summary>
		/// By close deal ticket
		/// </summary>
		/*[FieldOffset(680)]*/		public long ByCloseTicket;
		/*[FieldOffset(688)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 112)]*/		private byte[] s2B0;

		//new internal string GetString(byte[] buf)
		//{
		//	int count = 0;
		//	for (int i = 0; i < buf.Length; i += 2)
		//	{
		//		if (buf[i] == 0 && buf[i + 1] == 0)
		//			break;
		//		count++;
		//	}
		//	byte[] res = new byte[count * 2];
		//	for (int i = 0; i < count * 2; i++)
		//		res[i] = buf[i];
		//	string result = Encoding.Unicode.GetString(res);
		//	return result;
		//}

		internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 800;
			var st = new TradeRequest();
			st.RequestId = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s4 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.s4[i] = buf.Byte();
			st.TradeType = (TradeType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Login = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.s50 = new byte[72];
			for (int i = 0; i < 72; i++)
				st.s50[i] = buf.Byte();
			st.TransferLogin = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.sA0 = GetString(buf.Bytes(128));
			st.Currency = GetString(buf.Bytes(64));
			st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.s168 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Digits = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s174 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s17C = new byte[20];
			for (int i = 0; i < 20; i++)
				st.s17C[i] = buf.Byte();
			st.OrderTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s198 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.s198[i] = buf.Byte();
			st.s1D8 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.ExpirationTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.OrderType = (OrderType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.FillPolicy = (FillPolicy)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.ExpirationType = (ExpirationType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Flags = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.PlacedType = (PlacedType)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s200 = new byte[16];
			for (int i = 0; i < 16; i++)
				st.s200[i] = buf.Byte();
			st.Price = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.OrderPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.StopLoss = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.TakeProfit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Deviation = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.s238 = new byte[32];
			for (int i = 0; i < 32; i++)
				st.s238[i] = buf.Byte();
			st.ExpertId = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Comment = GetString(buf.Bytes(64));
			st.DealTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.ByCloseTicket = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s2B0 = new byte[112];
			for (int i = 0; i < 112; i++)
				st.s2B0[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
			return st;
		}

		 internal void WriteToBuf(OutBuf buf)
		{
			buf.Add(RequestId);
			buf.Add(s4, 64);
			buf.Add((int)TradeType);
			buf.Add(Login);
			buf.Add(s50, 72);
			buf.Add(TransferLogin);
			buf.Add(sA0, 32);
			buf.Add(Currency, 16);
			buf.Add(Volume);
			buf.Add(s168);
			buf.Add(Digits);
			buf.Add(s174);
			buf.Add(s17C, 20);
			buf.Add(OrderTicket);
			buf.Add(s198, 64);
			buf.Add(s1D8);
			buf.Add(ExpirationTime);
			buf.Add((int)OrderType);
			buf.Add((int)FillPolicy);
			buf.Add((int)ExpirationType);
			buf.Add(Flags);
			buf.Add((int)PlacedType);
			buf.Add(s200, 16);
			buf.Add(Price);
			buf.Add(OrderPrice);
			buf.Add(StopLoss);
			buf.Add(TakeProfit);
			buf.Add(Deviation);
			buf.Add(s238, 32);
			buf.Add(ExpertId);
			buf.Add(Comment, 16);
			buf.Add(DealTicket);
			buf.Add(ByCloseTicket);
			buf.Add(s2B0, 112);
		}
	}
}
