using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x88, CharSet = CharSet.Unicode)]*/
    class MailRecipient : FromBufReader
{
        /*[FieldOffset(0)]*/ public long Id;
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/ public string Name;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 136;
			var st = new MailRecipient();
			st.Id = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Name = GetString(buf.Bytes(128));
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
