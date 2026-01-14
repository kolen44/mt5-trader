using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xC0, CharSet = CharSet.Unicode)]*/
    class PumpDeals5D8 : FromBufReader
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private byte[] s0;
        /*[FieldOffset(8)]*/ public double Balance;
        /*[FieldOffset(16)]*/ public double Credit;
        /*[FieldOffset(24)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]*/ private byte[] s18;
        /*[FieldOffset(32)]*/ private double s20;
        /*[FieldOffset(40)]*/ private double s28;
        /*[FieldOffset(48)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 52)]*/ private byte[] s30;
        /*[FieldOffset(100)]*/ public double Blocked;
        /*[FieldOffset(108)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 84)]*/ private byte[] s6C;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 192;
			var st = new PumpDeals5D8();
			st.s0 = new byte[8];
			for (int i = 0; i < 8; i++)
				st.s0[i] = buf.Byte();
			st.Balance = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Credit = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s18 = new byte[8];
			for (int i = 0; i < 8; i++)
				st.s18[i] = buf.Byte();
			st.s20 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s28 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s30 = new byte[52];
			for (int i = 0; i < 52; i++)
				st.s30[i] = buf.Byte();
			st.Blocked = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s6C = new byte[84];
			for (int i = 0; i < 84; i++)
				st.s6C[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}

	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x40, CharSet = CharSet.Unicode)]*/
	class PumpDeals698 : FromBufReader
	{
		/*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/
		private byte[] s0;
		internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 64;
			var st = new PumpDeals698();
			st.s0 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.s0[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
			return st;
		}
	}
}
