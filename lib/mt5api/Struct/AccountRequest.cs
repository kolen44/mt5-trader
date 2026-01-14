using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	public class AccountRequest
	{
		//public byte[] ToBytes(AccRequest req)
		//{
		//	OutBuf buf = new OutBuf();
		//}

		/*[FieldOffset(0)]*/
		internal byte Random;
		/*[FieldOffset(1)]*/
		public byte Type;
		/*[FieldOffset(2)]*/
		internal short Revision;
		/*[FieldOffset(4)]*/
		internal short Signature;
		/*[FieldOffset(6)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/
		internal byte[] Key;
		/*[FieldOffset(22)]*/
		public int Time;
		/*[FieldOffset(26)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
		public string UserName;
		/*[FieldOffset(282)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string AccType;
		/*[FieldOffset(410)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		internal string s19A = null;
		/*[FieldOffset(474)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string Country;
		/*[FieldOffset(538)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string City;
		/*[FieldOffset(602)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string State;
		/*[FieldOffset(666)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/
		public string ZipCode;
		/*[FieldOffset(698)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
		public string Address;
		/*[FieldOffset(954)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string Phone;
		/*[FieldOffset(1018)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string Email;
		/*[FieldOffset(1146)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string CompanyName;
		/*[FieldOffset(1274)]*/
		public double Deposit;
		/*[FieldOffset(1282)]*/
		public int Leverage;
		/*[FieldOffset(1286)]*/
		public int LanguageId;
		/*[FieldOffset(1290)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string UtmCampaign;
        /*[FieldOffset(1354)]*/
        public int Flags;           //54A
        public int PushID;          //54E
		public byte[] NetAddr;          //552
        public uint AgreeFlags;        //566
        public byte[] RanddomTail;		//56A
        /*[FieldOffset(1358)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 60)]*/
        //internal byte[] s54E;
	}
}
