using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x5A, CharSet = CharSet.Unicode)]*/
    class Ticker : FromBufReader
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Name;
        /*[FieldOffset(64)]*/ public short BankId;
        /*[FieldOffset(66)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]*/ private byte[] s42;

    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 90;
			var st = new Ticker();
			st.Name = GetString(buf.Bytes(64));
			st.BankId = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.s42 = new byte[24];
			for (int i = 0; i < 24; i++)
				st.s42[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
