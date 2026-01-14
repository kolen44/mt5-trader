using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x504, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Server information from servers.dat
    /// </summary>
    public class AddressRecEx : FromBufReader, ToBufWriter
    {
        internal static readonly int Size = 1284;
        /*[FieldOffset(0)]*/
        private int s0;
        /*[FieldOffset(4)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/
        private string s4;
        /*[FieldOffset(516)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/
        private string s204;
        /*[FieldOffset(1028)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]*/
        private byte[] s404 = new byte[256];
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 1284;
            var st = new AddressRecEx();
            st.s0 = BitConverter.ToInt32(buf.Bytes(4), 0);
            st.s4 = GetString(buf.Bytes(512));
            st.s204 = GetString(buf.Bytes(512));
            st.s404 = new byte[256];
            for (int i = 0; i < 256; i++)
                st.s404[i] = buf.Byte();
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
            return st;
        }

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;
            buf.Add(s0);
            buf.Add(GetBytes(s4, 512));
            buf.Add(GetBytes(s204, 512));
            buf.Add(s404);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}
