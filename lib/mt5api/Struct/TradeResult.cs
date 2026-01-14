using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x104, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Trade result
	/// </summary>
	public class TradeResult : FromBufReader
    {
        /*[FieldOffset(0)]*/ public Msg Status;
        /*[FieldOffset(4)]*/ public long PositionId;
        /*[FieldOffset(12)]*/ public long TicketNumber;
        /*[FieldOffset(20)]*/ public ulong Volume;
        /*[FieldOffset(28)]*/ public double OpenPrice;
        /*[FieldOffset(36)]*/ private int s0;
        /*[FieldOffset(40)]*/ private int s4;
        /*[FieldOffset(44)]*/ public double Bid;
        /*[FieldOffset(52)]*/ public double Ask;
        /*[FieldOffset(60)]*/ public double Last;
        /*[FieldOffset(68)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Comment;
        /*[FieldOffset(132)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/ private byte[] s84;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 260;
			var st = new TradeResult();
			st.Status = (Msg)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.PositionId = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.TicketNumber = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.Volume = BitConverter.ToUInt64(buf.Bytes(8), 0);
			st.OpenPrice = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.s0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Bid = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Ask = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Last = BitConverter.ToDouble(buf.Bytes(8), 0);
			st.Comment = GetString(buf.Bytes(64));
			st.s84 = new byte[128];
			for (int i = 0; i < 128; i++)
				st.s84[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}
}
