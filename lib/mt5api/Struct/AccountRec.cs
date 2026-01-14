using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xAB4, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Account details
	/// </summary>
    public class AccountRec : FromBufReader
    {
        /*[FieldOffset(0)]*/ public ulong Login;
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private byte[] s8;
        /*[FieldOffset(16)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string Type;
        /*[FieldOffset(48)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 152)]*/ private byte[] s30;
        /*[FieldOffset(200)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string UserName;
		//#define vDISABLE_ON_SERVER			4
		//#define vINVESTOR_MODE				8
		//#define vALLOWED_TRAILING			0x20
		//#define vALLOWED_AUTOTRADE			0x40
		//#define vACCOUNT_NOT_CONFIRMED		0x200
		//#define vPASS_MUST_BY_CHANGED		0x400
		/*[FieldOffset(456)]*/
		public int TradeFlags;
        /*[FieldOffset(460)]*/ private int s1CC;
        /*[FieldOffset(464)]*/ private int s1D0;
        /*[FieldOffset(468)]*/ private int s1D4;
        /*[FieldOffset(472)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 56)]*/ private byte[] s1D8;
        /*[FieldOffset(528)]*/ private int s210;
        /*[FieldOffset(532)]*/ private int s214;
        /*[FieldOffset(536)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private byte[] s218;
        /*[FieldOffset(544)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ private string s220;
        /*[FieldOffset(608)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ private string s260;
        /*[FieldOffset(672)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Country;
        /*[FieldOffset(736)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string City;
        /*[FieldOffset(800)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string State;
        /*[FieldOffset(864)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string ZipCode;
        /*[FieldOffset(896)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string UserAddress;
        /*[FieldOffset(1152)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Phone;
        /*[FieldOffset(1216)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ public string Email;
        /*[FieldOffset(1344)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s540;
        /*[FieldOffset(1472)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ private string s5C0;
        /*[FieldOffset(1536)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ private string s600;
        /*[FieldOffset(1568)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s620;
        /*[FieldOffset(1696)]*/ private int s6A0;
        /*[FieldOffset(1700)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 136)]*/ private byte[] s6A4;
        /*[FieldOffset(1836)]*/ public double Balance;
        /*[FieldOffset(1844)]*/ public double Credit;
        /*[FieldOffset(1852)]*/ private double s73C;
        /*[FieldOffset(1860)]*/ private double s744;
        /*[FieldOffset(1868)]*/ private double s74C;
        /*[FieldOffset(1876)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 52)]*/ private byte[] s754;
        /*[FieldOffset(1928)]*/ public double Blocked;
        /*[FieldOffset(1936)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 84)]*/ private byte[] s790;
        /*[FieldOffset(2020)]*/ public int Leverage;
        /*[FieldOffset(2024)]*/ private int s7E8;
        /*[FieldOffset(2028)]*/ private int s7EC;
        /*[FieldOffset(2032)]*/ private int s7F0;
        /*[FieldOffset(2036)]*/ private double s7F4;
        /*[FieldOffset(2044)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 312)]*/ private byte[] s7FC;
        /*[FieldOffset(2356)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] s934;
        /*[FieldOffset(2484)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]*/ private byte[] s9B4;
		private byte[] sAB4;


        internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 0xBB4;
			var st = new AccountRec();
			st.Login = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.s8 = new byte[8];
			for (int i = 0; i < 8; i++)
				st.s8[i] = buf.Byte();
			st.Type = GetString(buf.Bytes(32));
			st.s30 = new byte[152];
			for (int i = 0; i < 152; i++)
				st.s30[i] = buf.Byte();
			st.UserName = GetString(buf.Bytes(256));
			st.TradeFlags = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1CC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1D0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1D4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1D8 = new byte[56];
			for (int i = 0; i < 56; i++)
				st.s1D8[i] = buf.Byte();
			st.s210 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s214 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s218 = new byte[8];
			for (int i = 0; i < 8; i++)
				st.s218[i] = buf.Byte();
			st.s220 = GetString(buf.Bytes(64));
			st.s260 = GetString(buf.Bytes(64));
			st.Country = GetString(buf.Bytes(64));
			st.City = GetString(buf.Bytes(64));
			st.State = GetString(buf.Bytes(64));
			st.ZipCode = GetString(buf.Bytes(32));
			st.UserAddress = GetString(buf.Bytes(256));
			st.Phone = GetString(buf.Bytes(64));
			st.Email = GetString(buf.Bytes(128));
			st.s540 = GetString(buf.Bytes(128));
			st.s5C0 = GetString(buf.Bytes(64));
			st.s600 = GetString(buf.Bytes(32));
			st.s620 = GetString(buf.Bytes(128));
			st.s6A0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s6A4 = new byte[136];
			for (int i = 0; i < 136; i++)
				st.s6A4[i] = buf.Byte();
			st.Balance = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Credit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s73C = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s744 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s74C = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s754 = new byte[52];
			for (int i = 0; i < 52; i++)
				st.s754[i] = buf.Byte();
			st.Blocked = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s790 = new byte[84];
			for (int i = 0; i < 84; i++)
				st.s790[i] = buf.Byte();
			st.Leverage = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s7E8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s7EC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s7F0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s7F4 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s7FC = new byte[312];
			for (int i = 0; i < 312; i++)
				st.s7FC[i] = buf.Byte();
			st.s934 = new byte[128];
			for (int i = 0; i < 128; i++)
				st.s934[i] = buf.Byte();
			st.s9B4 = new byte[256];
			for (int i = 0; i < 256; i++)
				st.s9B4[i] = buf.Byte();
			st.sAB4 = new byte[256];
			for (int i = 0; i < 256; i++)
				st.sAB4[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
