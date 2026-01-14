using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x28, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Trade or quote session details
    /// </summary>
    public class Session : FromBufReader
    {
        /// <summary>
        /// Start time (in minites)
        /// </summary>
        /*[FieldOffset(0)]*/
        public int StartTime;
        /// <summary>
        /// End time (in minutes)
        /// </summary>
        /*[FieldOffset(4)]*/
        public int EndTime;
        /*[FieldOffset(8)]*/
        private int s8;
        /*[FieldOffset(12)]*/
        private int sC;
        /*[FieldOffset(16)]*/
        private int s10;
        /*[FieldOffset(20)]*/
        private int s14;
        /*[FieldOffset(24)]*/
        private int s18;
        /*[FieldOffset(28)]*/
        private int s1C;
        /*[FieldOffset(32)]*/
        private int s20;
        /*[FieldOffset(36)]*/
        private int s24;
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 40;
            var st = new Session();
            st.StartTime = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.EndTime = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s8 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.sC = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s10 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s14 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s18 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s1C = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s20 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s24 = BitConverter.ToInt32(buf.Bytes(4), 0);
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
            return st;
        }
    }
}
