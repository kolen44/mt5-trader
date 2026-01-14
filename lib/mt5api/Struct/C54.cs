using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x50, CharSet = CharSet.Unicode)]*/
    class SymTicks : FromBufReader
    {
        /*[FieldOffset(0)]*/ private long s0;
        /*[FieldOffset(8)]*/ private long s8;
        /*[FieldOffset(16)]*/ private int s10;
        /*[FieldOffset(20)]*/ private int s14;
        /*[FieldOffset(24)]*/ private int s18;
        /*[FieldOffset(28)]*/ private int s1C;
        /*[FieldOffset(32)]*/ private long s20;
        /*[FieldOffset(40)]*/ private int s28;
        /*[FieldOffset(44)]*/ private int s2C;
        /*[FieldOffset(48)]*/ private int s30;
        /*[FieldOffset(52)]*/ private int s34;
        /*[FieldOffset(56)]*/ private int s38;
        /*[FieldOffset(60)]*/ private int s3C;
        /*[FieldOffset(64)]*/ private int s40;
        /*[FieldOffset(68)]*/ private int s44;
        /*[FieldOffset(72)]*/ private int s48;
        /*[FieldOffset(76)]*/ private int s4C;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 80;
			var st = new SymTicks();
			st.s0 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s8 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s10 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s14 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s18 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s20 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s28 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s2C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s30 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s34 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s38 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s3C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s40 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s44 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s48 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s4C = BitConverter.ToInt32(buf.Bytes(4), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
