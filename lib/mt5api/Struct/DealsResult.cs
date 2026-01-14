using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x18B, CharSet = CharSet.Unicode)]*/
    class DealsResult : FromBufReader
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 395)]*/ private byte[] s0;
		int Size = 396;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + Size;
			var st = new DealsResult();
			st.s0 = new byte[Size];
			for (int i = 0; i < Size; i++)
				st.s0[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
