using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x1AC, CharSet = CharSet.Unicode)]*/
    public class DatHeader : FromBufReader, ToBufWriter
    {
        /*[FieldOffset(0)]*/ public uint Id = 506;
        /*[FieldOffset(4)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
        public string Copyright = "Copyright 2000-2025, MetaQuotes Ltd.";
        /*[FieldOffset(132)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]*/ public string DataType = "Servers";
        /*[FieldOffset(164)]*/
        public long FileTime;
        /*[FieldOffset(172)]*/ public int ObjNumber;
        /*[FieldOffset(176)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ public byte[] Md5Key = new byte[16];
        /*[FieldOffset(192)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 228)]*/ private byte[] sC0 = new byte[228];
        /*[FieldOffset(420)]*/ private int s1A4;
        /*[FieldOffset(424)]*/ private int s1A8;
		public static readonly int Size = 428; 

    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + Size;
			var st = new DatHeader();
			st.Id = BitConverter.ToUInt32(buf.Bytes(4), 0);
			st.Copyright = GetString(buf.Bytes(128));
			st.DataType = GetString(buf.Bytes(32));
			st.FileTime = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.ObjNumber = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Md5Key = new byte[16];
			for (int i = 0; i < 16; i++)
				st.Md5Key[i] = buf.Byte();
			st.sC0 = new byte[228];
			for (int i = 0; i < 228; i++)
				st.sC0[i] = buf.Byte();
			st.s1A4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s1A8 = BitConverter.ToInt32(buf.Bytes(4), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;

            buf.Add(Id);
            buf.Add(GetBytes(Copyright, 128));  // 64 UTF-16 chars = 128 bytes
            buf.Add(GetBytes(DataType, 32));    // 16 UTF-16 chars = 32 bytes
            buf.Add(FileTime);
            buf.Add(ObjNumber);
            buf.Add(Md5Key);
            buf.Add(sC0);
            buf.Add(s1A4);
            buf.Add(s1A8);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}
