using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xE84, CharSet = CharSet.Unicode)]*/
    /// <summary>
	/// Base symbol information
	/// </summary>
	public class SymBaseInfo : FromBufReader, IEqualityComparer<SymBaseInfo>, IEqualityComparer
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ private byte[] s0;
        /*[FieldOffset(16)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s10;
        /*[FieldOffset(144)]*/ private int s90;
        /*[FieldOffset(148)]*/ private long s94;
        /*[FieldOffset(156)]*/ internal long s9C;
        /*[FieldOffset(164)]*/ private long sA4;
        /*[FieldOffset(172)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] sAC;
        /*[FieldOffset(236)]*/ private int sEC;
        /*[FieldOffset(240)]*/ private long sF0;
        /*[FieldOffset(248)]*/ private int sF8;
        /*[FieldOffset(252)]*/ private int sFC;
        /*[FieldOffset(256)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 60)]*/ private byte[] s100;
        /*[FieldOffset(316)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string CompanyName;
        /*[FieldOffset(572)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/ private string s23C;
        /*[FieldOffset(1084)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s43C;
        /*[FieldOffset(1212)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/ private string s4BC;
        /*[FieldOffset(1724)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s6BC;
        /*[FieldOffset(1852)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s73C;
        /*[FieldOffset(1980)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s7BC;
		private string s8BC;
		private string s9BC;

		/*[FieldOffset(2108)]*/
		private int s83C;
        /*[FieldOffset(2112)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s840;
        /*[FieldOffset(2240)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s8C0;
        /*[FieldOffset(2368)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s940;
        /*[FieldOffset(2496)]*/ private long s9C0;
        /*[FieldOffset(2504)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] s9C8;
        /*[FieldOffset(2568)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Currency;
        /*[FieldOffset(2632)]*/ public int Digits;
        /*[FieldOffset(2636)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] sA4C;
        /*[FieldOffset(2764)]*/ private int sACC;
        /*[FieldOffset(2768)]*/ private int sAD0;
        /*[FieldOffset(2772)]*/ private int sAD4;
        /*[FieldOffset(2776)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string sAD8;
        /*[FieldOffset(2904)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private int[] sB58;
        /*[FieldOffset(2936)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] sB78;
        /*[FieldOffset(2968)]*/ public int ReceiveUserMsg;
        /*[FieldOffset(2972)]*/ private int sB9C;
        /*[FieldOffset(2976)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] sBA0;
        /*[FieldOffset(3040)]*/ public AccMethod AccMethod;
        /*[FieldOffset(3044)]*/ private int sBE4;
        /*[FieldOffset(3048)]*/ private int sBE8;
        /*[FieldOffset(3052)]*/ public MarginMode MarginMode;
        /*[FieldOffset(3056)]*/ private double sBF0;
        /*[FieldOffset(3064)]*/ private double sBF8;
        /*[FieldOffset(3072)]*/ private int sC00;
        /*[FieldOffset(3076)]*/ private double sC04;
        /*[FieldOffset(3084)]*/ private int sC0C;
        /*[FieldOffset(3088)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 60)]*/ private byte[] sC10;
        /*[FieldOffset(3148)]*/ private int sC4C;
        /*[FieldOffset(3152)]*/ private double sC50;
        /*[FieldOffset(3160)]*/ private int sC58;
        /*[FieldOffset(3164)]*/ private int sC5C;
        /*[FieldOffset(3168)]*/ private int sC60;
        /*[FieldOffset(3172)]*/ private double sC64;
        /*[FieldOffset(3180)]*/ private long sC6C;
        /*[FieldOffset(3188)]*/ private int sC74;
        /*[FieldOffset(3192)]*/ private int sC78;
        /*[FieldOffset(3196)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 248)]*/ private byte[] sC7C;
        /*[FieldOffset(3444)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ public byte[] SymbolsHash;
        /*[FieldOffset(3460)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] sD84;
        /*[FieldOffset(3492)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ public byte[] SpreadsHash;
        /*[FieldOffset(3508)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 208)]*/ private byte[] sDB4;
        internal static int Size = 0xE84;

        internal override object ReadFromBuf(InBuf buf)
		{
			int endInd;
			if(buf.SymBuild <= 2097)
				endInd = buf.CurrentIndex + 3716;
			else
				endInd = buf.CurrentIndex + 4228;
			var st = new SymBaseInfo();
			st.s0 = new byte[16];
			for (int i = 0; i < 16; i++)
				st.s0[i] = buf.Byte();
			st.s10 = GetString(buf.Bytes(128));
			st.s90 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s94 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s9C = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.sA4 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.sAC = new byte[64];
			for (int i = 0; i < 64; i++)
				st.sAC[i] = buf.Byte();
			st.sEC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sF0 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.sF8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sFC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s100 = new byte[60];
			for (int i = 0; i < 60; i++)
				st.s100[i] = buf.Byte();
			st.CompanyName = GetString(buf.Bytes(256));
			st.s23C = GetString(buf.Bytes(512));
			st.s43C = GetString(buf.Bytes(128));
			st.s4BC = GetString(buf.Bytes(512));
			st.s6BC = GetString(buf.Bytes(128));
			st.s73C = GetString(buf.Bytes(128));
			if (buf.SymBuild <= 2097)
			{
				st.s7BC = GetString(buf.Bytes(128));
			}
			else
			{
				st.s7BC = GetString(buf.Bytes(256));
				st.s8BC = GetString(buf.Bytes(256));
				st.s9BC = GetString(buf.Bytes(128));
			}
			st.s83C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s840 = GetString(buf.Bytes(128));
			st.s8C0 = GetString(buf.Bytes(128));
			st.s940 = GetString(buf.Bytes(128));
			st.s9C0 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s9C8 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.s9C8[i] = buf.Byte();
			st.Currency = GetString(buf.Bytes(64));
			if (st.Currency == "")
				throw new Exception("Account currency is not correct");
			st.Digits = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sA4C = new byte[128];
			for (int i = 0; i < 128; i++)
				st.sA4C[i] = buf.Byte();
			st.sACC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sAD0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sAD4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sAD8 = GetString(buf.Bytes(128));
			st.sB58 = new int[8];
			for (int i = 0; i < 8; i++)
				st.sB58[i] = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sB78 = new byte[32];
			for (int i = 0; i < 32; i++)
				st.sB78[i] = buf.Byte();
			st.ReceiveUserMsg = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sB9C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sBA0 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.sBA0[i] = buf.Byte();
			st.AccMethod = (AccMethod)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sBE4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sBE8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.MarginMode = (MarginMode)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sBF0 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.sBF8 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.sC00 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC04 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.sC0C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC10 = new byte[60];
			for (int i = 0; i < 60; i++)
				st.sC10[i] = buf.Byte();
			st.sC4C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC50 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.sC58 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC5C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC60 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC64 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.sC6C = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.sC74 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC78 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC7C = new byte[248];
			for (int i = 0; i < 248; i++)
				st.sC7C[i] = buf.Byte();
			st.SymbolsHash = new byte[16];
			for (int i = 0; i < 16; i++)
				st.SymbolsHash[i] = buf.Byte();
			st.sD84 = new byte[32];
			for (int i = 0; i < 32; i++)
				st.sD84[i] = buf.Byte();
			st.SpreadsHash = new byte[16];
			for (int i = 0; i < 16; i++)
				st.SpreadsHash[i] = buf.Byte();
			st.sDB4 = new byte[208];
			for (int i = 0; i < 208; i++)
				st.sDB4[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex + " != "+endInd);
			return st;
		}

        public static bool AreEqual(SymBaseInfo sbi1, SymBaseInfo sbi2)
        {
            // Compare all public fields
            if (!string.Equals(sbi1.CompanyName, sbi2.CompanyName) ||
                !string.Equals(sbi1.Currency, sbi2.Currency) ||
                !AreByteArraysEqual(sbi1.SymbolsHash, sbi2.SymbolsHash) ||
                !AreByteArraysEqual(sbi1.SpreadsHash, sbi2.SpreadsHash))
            {
                return false;
            }

			// Compare public and internal fields
			return sbi1.Digits == sbi2.Digits &&
				   sbi1.ReceiveUserMsg == sbi2.ReceiveUserMsg &&
				   sbi1.AccMethod == sbi2.AccMethod &&
				   sbi1.MarginMode == sbi2.MarginMode &&
				   sbi1.s9C == sbi2.s9C;
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
            return AreEqual((SymBaseInfo)x, (SymBaseInfo)y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(SymBaseInfo x, SymBaseInfo y)
        {
            return AreEqual((SymBaseInfo)x, (SymBaseInfo)y);
        }

        public int GetHashCode(SymBaseInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}

