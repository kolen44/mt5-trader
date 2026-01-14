using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x24, CharSet = CharSet.Unicode)]*/
    internal class ZipRecord : FromBufReader
    {
        /*[FieldOffset(0)]*/
        public uint Offset;
        /*[FieldOffset(4)]*/
        public uint PackSize;
        /*[FieldOffset(8)]*/
        public uint DataSize;
        /*[FieldOffset(12)]*/
        public int Type;
        /*[FieldOffset(16)]*/
        public uint Crc32;
        /*[FieldOffset(20)]*/
        public int s14;
        /*[FieldOffset(24)]*/
        public int s18;
        /*[FieldOffset(28)]*/
        public int s1C;
        /*[FieldOffset(32)]*/
        public int s20;
		public byte[] Data;
		internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 36;
            var st = new ZipRecord();
            st.Offset = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.PackSize = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.DataSize = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.Type = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.Crc32 = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.s14 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s18 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s1C = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s20 = BitConverter.ToInt32(buf.Bytes(4), 0);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}
