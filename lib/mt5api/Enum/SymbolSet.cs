using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x188, CharSet = CharSet.Unicode)]*/
    class SymbolSet : FromBufReader
    {
        /*[FieldOffset(0)]*/ public long UpdateTime;
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/ public string GroupNames;
        /*[FieldOffset(264)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ private string s108;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 392;
			var st = new SymbolSet();
			st.UpdateTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.GroupNames = GetString(buf.Bytes(256));
			st.s108 = GetString(buf.Bytes(128));
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
