using mtapi.mt5.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace mtapi.mt5
{
    [Flags]
    public enum HeaderFlags : ushort
    {
        COMPRESSED = 1,
        // if (Flags & (TCH_ZIP_BY_HOURS | TCH_ZIP_PACKED)) == 0 then use vBitReader
        ZIP_BY_HOURS = 2,
        ZIP_PACKED = 4,
        NEED_UPDATE	= 8
    }

    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x77, CharSet = CharSet.Unicode)]*/
    public class ContainerHeader : FromBufReader
    {
        /*[FieldOffset(0)]*/
        public ushort HdrSize;
        /*[FieldOffset(2)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        public string Currency;
        /*[FieldOffset(66)]*/
        public ushort Date;
        /*[FieldOffset(68)]*/
        public ushort s44;
        /*[FieldOffset(70)]*/
        public int DataSize;
        /*[FieldOffset(74)]*/
        public int InflateSize;
        /*[FieldOffset(78)]*/
        public int BitSize;
        /*[FieldOffset(82)]*/
        public int NumberTicks;
        /*[FieldOffset(86)]*/
        public byte AlignBit;
        /*[FieldOffset(87)]*/
        public int Time;
        /*[FieldOffset(91)]*/
        public ushort Digits;
        /*[FieldOffset(93)]*/
        public HeaderFlags Flags;
        /*[FieldOffset(95)]*/
        public uint LimitPoints;
        /*[FieldOffset(99)]*/
        public int s63;
        /*[FieldOffset(103)]*/
        public uint Crc32;
        /*[FieldOffset(107)]*/
        public int LastVolumeTicks;
        /*[FieldOffset(111)]*/
        public int s6F;
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 115;
            var st = new ContainerHeader();
            st.HdrSize = BitConverter.ToUInt16(buf.Bytes(2), 0);
            st.Currency = GetString(buf.Bytes(64));
            st.Date = BitConverter.ToUInt16(buf.Bytes(2), 0);
            st.s44 = BitConverter.ToUInt16(buf.Bytes(2), 0);
            st.DataSize = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.InflateSize = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.BitSize = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.NumberTicks = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.AlignBit = buf.Byte();
            st.Time = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.Digits = BitConverter.ToUInt16(buf.Bytes(2), 0);
            st.Flags = (HeaderFlags)BitConverter.ToUInt16(buf.Bytes(2), 0);
            st.LimitPoints = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.s63 = BitConverter.ToInt32(buf.Bytes(4), 0); 
            st.Crc32 = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.LastVolumeTicks = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s6F = BitConverter.ToInt32(buf.Bytes(4), 0); 
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}