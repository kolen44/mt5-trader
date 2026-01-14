using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x10C, CharSet = CharSet.Unicode)]*/
    /// <summary>
	/// Server name from servers.dat
	/// </summary>
	public class AccessInfo : FromBufReader
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string ServerName;
        /*[FieldOffset(64)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 36)]*/ private byte[] s40;
        /*[FieldOffset(100)]*/ private int s64;
        /*[FieldOffset(104)]*/ private int s68;
        /*[FieldOffset(108)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 160)]*/ private byte[] s6C;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 268;
			var st = new AccessInfo();
			st.ServerName = GetString(buf.Bytes(64));
			st.s40 = new byte[36];
			for (int i = 0; i < 36; i++)
				st.s40[i] = buf.Byte();
			st.s64 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s68 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s6C = new byte[160];
			for (int i = 0; i < 160; i++)
				st.s6C[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
