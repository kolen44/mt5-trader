using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x34, CharSet = CharSet.Unicode)]*/
    internal class ZipInfo : FromBufReader
    {
        /*[FieldOffset(0)]*/
        public uint HdrSize;
        /*[FieldOffset(4)]*/
        public uint DataSize;
        /*[FieldOffset(8)]*/
        public uint ObjNumber;
        /*[FieldOffset(12)]*/
        public uint Crc32;
        /*[FieldOffset(16)]*/
        public int s10;
        /*[FieldOffset(20)]*/
        public int s14;
        /*[FieldOffset(24)]*/
        public int s18;
        /*[FieldOffset(28)]*/
        public int s1C;
        /*[FieldOffset(32)]*/
        public int s20;
        /*[FieldOffset(36)]*/
        public int s24;
        /*[FieldOffset(40)]*/
        public int s28;
        /*[FieldOffset(44)]*/
        public int s2C;
        /*[FieldOffset(48)]*/
        public int NumRecords;
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 52;
            var st = new ZipInfo();
            st.HdrSize = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.DataSize = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.ObjNumber = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.Crc32 = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.s10 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s14 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s18 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s1C = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s20 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s24 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s28 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s2C = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.NumRecords = BitConverter.ToInt32(buf.Bytes(4), 0);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}
