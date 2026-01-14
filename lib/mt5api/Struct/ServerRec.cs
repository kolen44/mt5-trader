using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x214, CharSet = CharSet.Unicode)]*/
    /// <summary>
	/// Server details from servers.dat
	/// </summary>
	public class ServerRec : FromBufReader
    {
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ public string ServerName;
        /*[FieldOffset(128)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string CompanyName;
        /*[FieldOffset(384)]*/ private int s180;
        /*[FieldOffset(388)]*/ private int SymBuild;
        /*[FieldOffset(392)]*/ public int DST;
        /*[FieldOffset(396)]*/ public int TimeZone;
        /*[FieldOffset(400)]*/ private int Demo;
        /*[FieldOffset(404)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] s194;

    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 532;
			var st = new ServerRec();
			st.ServerName = GetString(buf.Bytes(128));
			st.CompanyName = GetString(buf.Bytes(256));
			st.s180 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.SymBuild = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.DST = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TimeZone = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Demo = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s194 = new byte[128];
			for (int i = 0; i < 128 ; i++)
				st.s194[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
