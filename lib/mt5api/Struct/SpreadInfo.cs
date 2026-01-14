using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x64, CharSet = CharSet.Unicode)]*/
    class SpreadInfo : FromBufReader
{
        /*[FieldOffset(0)]*/ public long Time;
        /*[FieldOffset(8)]*/ public int Id;
        /*[FieldOffset(12)]*/ private int sC;
        /*[FieldOffset(16)]*/ private double s10;
        /*[FieldOffset(24)]*/ private double s18;
        /*[FieldOffset(32)]*/ private int s20;
        /*[FieldOffset(36)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]*/ private byte[] s24;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 100;
			var st = new SpreadInfo();
			st.Time = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Id = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s10 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s18 = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s20 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s24 = new byte[64];
			for (int i = 0; i < 64; i++)
				st.s24[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
