using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x81, CharSet = CharSet.Unicode)]*/
	class HistHeader                     : FromBufReader
    {
        /*[FieldOffset(0)]*/ public short HdrSize;
        /*[FieldOffset(2)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/ public string Currency;
        /*[FieldOffset(66)]*/public short s42;
        /*[FieldOffset(68)]*/ public short Date;
        /*[FieldOffset(70)]*/ public short s46;
        /*[FieldOffset(72)]*/ public int DataSize;
        /*[FieldOffset(76)]*/ public int InflateSize;
        /*[FieldOffset(80)]*/ public int BitSize;
        /*[FieldOffset(84)]*/ public int NumberBars;
        /*[FieldOffset(88)]*/ public byte AlignBit;
        /*[FieldOffset(89)]*/ public int Time;
        /*[FieldOffset(93)]*/ public short Digits;
        /*[FieldOffset(95)]*/ public short Flags;
        /*[FieldOffset(97)]*/ public uint LimitPoints;
        /*[FieldOffset(101)]*/ public int Spread;
        /*[FieldOffset(105)]*/ public float fSwapLong;
        /*[FieldOffset(109)]*/ public float fSwapShort;
        /*[FieldOffset(113)]*/ private long s71;
        /*[FieldOffset(121)]*/ private long s79;
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 129;
			var st = new HistHeader                    ();
			st.HdrSize = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.Currency = GetString(buf.Bytes(64));
			st.s42 = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.Date = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.s46 = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.DataSize = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.InflateSize = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.BitSize = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.NumberBars = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.AlignBit = buf.Byte();
			st.Time = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Digits = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.Flags = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.LimitPoints = BitConverter.ToUInt32(buf.Bytes(4), 0);
			st.Spread = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.fSwapLong = (float)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.fSwapShort = (float)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s71 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s79 = BitConverter.ToInt64(buf.Bytes(8), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
};
}
