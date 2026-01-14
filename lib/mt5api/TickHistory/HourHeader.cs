using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace mtapi.mt5
{
    internal class HourHeader : FromBufReader
    {
        public uint DataSize;              //0
        public uint ObjNumber;             //4
        public uint Crc32;                 //8
        public int sC;

        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 16;
            var st = new HourHeader();
            st.DataSize = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.ObjNumber = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.Crc32 = BitConverter.ToUInt32(buf.Bytes(4), 0);
            st.sC = BitConverter.ToInt32(buf.Bytes(4), 0);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
            return st;
        }
    }
}
