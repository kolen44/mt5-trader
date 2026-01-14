using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Size = 0x09, CharSet = CharSet.Unicode)]*/
	class PacketHdr : FromBufReader//sizeof 0x09 c
	{
        /*[FieldOffset(0)]*/        public byte Type; //0
        /*[FieldOffset(1)]*/        public int PacketSize; //1
        /*[FieldOffset(5)]*/        public ushort Id; //5
        /*[FieldOffset(7)]*/        public ushort Flags; //7
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 9;
			var st = new PacketHdr(); //sizeof 0x09 c();
			st.Type = buf.Byte();
			st.PacketSize = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Id = BitConverter.ToUInt16(buf.Bytes(2), 0);
			st.Flags = BitConverter.ToUInt16(buf.Bytes(2), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
