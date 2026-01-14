using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x4E, CharSet = CharSet.Unicode)]*/
    class TickRec : FromBufReader
    {
        /*[FieldOffset(0)]*/ public int Id;
        /*[FieldOffset(4)]*/ public long Time;
        /*[FieldOffset(12)]*/ public long TimeMs;
        /*[FieldOffset(20)]*/ public ulong UpdateMask;
        /*[FieldOffset(28)]*/ public long Bid;
        /*[FieldOffset(36)]*/ public long Ask;
        /*[FieldOffset(44)]*/ public long Last;
        /*[FieldOffset(52)]*/ public ulong Volume;
        /*[FieldOffset(60)]*/ internal long s3C;
        /*[FieldOffset(68)]*/ internal long s44;
        /*[FieldOffset(76)]*/ public short BankId;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 78;
			var st = new TickRec();
			st.Id = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Time = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.TimeMs = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.UpdateMask = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.Bid = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Ask = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Last = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.s3C = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s44 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.BankId = BitConverter.ToInt16(buf.Bytes(2), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
