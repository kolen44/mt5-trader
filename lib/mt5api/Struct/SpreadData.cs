using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x80, CharSet = CharSet.Unicode)]*/
    class SpreadData : FromBufReader
{
        /*[FieldOffset(0)]*/ private int s0;
        /*[FieldOffset(4)]*/ private int s4;
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Currency;
        /*[FieldOffset(72)]*/ private long s48;
        /*[FieldOffset(80)]*/ private long s50;
        /*[FieldOffset(88)]*/ private double s58;
        /*[FieldOffset(96)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]*/ private byte[] s60;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 128;
			var st = new SpreadData();
			st.s0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Currency = GetString(buf.Bytes(64));
			st.s48 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s50 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s58 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s60 = new byte[32];
			for (int i = 0; i < 32; i++)
				st.s60[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
