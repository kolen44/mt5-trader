using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0xC58, CharSet = CharSet.Unicode)]*/
    /// <summary>
    /// Server information from servers.dat
    /// </summary>
    public class AccessRecEx : FromBufReader, ToBufWriter
    {
        internal static readonly int Size = 3160;
        /*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
        private string s0;
        /*[FieldOffset(128)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
        string s80;
        /*[FieldOffset(256)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
        string s100;
        /*[FieldOffset(512)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
        string s200;
        /*[FieldOffset(576)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]*/
        byte[] s240 = new byte[24];
        /*[FieldOffset(600)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
        string s258;
        /*[FieldOffset(856)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1024)]*/
        string s358;
        /*[FieldOffset(2904)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]*/
        byte[] sB58 = new byte[256];
        internal override object ReadFromBuf(InBuf buf)
        {
            var endInd = buf.CurrentIndex + 3160;
            var st = new AccessRecEx();
            st.s0 = GetString(buf.Bytes(128));
            st.s80 = GetString(buf.Bytes(128));
            st.s100 = GetString(buf.Bytes(256));
            st.s200 = GetString(buf.Bytes(64));
            st.s240 = new byte[24];
            for (int i = 0; i < 24; i++)
                st.s240[i] = buf.Byte();
            st.s258 = GetString(buf.Bytes(256));
            st.s358 = GetString(buf.Bytes(2048));
            st.sB58 = new byte[256];
            for (int i = 0; i < 256; i++)
                st.sB58[i] = buf.Byte();
            if (buf.CurrentIndex != endInd)
                throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
            return st;
        }

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;
            buf.Add(GetBytes(s0, 128));
            buf.Add(GetBytes(s80, 128));
            buf.Add(GetBytes(s100, 256));
            buf.Add(GetBytes(s200, 64));
            buf.Add(s240);
            buf.Add(GetBytes(s258, 256));
            buf.Add(GetBytes(s358, 2048));
            buf.Add(sB58);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}