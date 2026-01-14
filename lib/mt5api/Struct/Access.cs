using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x164, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// ServerName from servers.dat
	/// </summary>
	public class AccessRec : FromBufReader, ToBufWriter
	{
		internal static readonly int Size = 356;
		/*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]*/
		public string ServerName;
        /*[FieldOffset(64)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]*/
        internal byte[] s40 = new byte[128];
        /*[FieldOffset(192)]*/
        internal int sC0;
        /*[FieldOffset(196)]*/
        internal int sC4 = 2177;
        /*[FieldOffset(200)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 156)]*/
        internal byte[] sC8 = new byte[156];
		internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 356;
			var st = new AccessRec();
			st.ServerName = GetString(buf.Bytes(64));
			st.s40 = new byte[128];
			for (int i = 0; i < 128; i++)
				st.s40[i] = buf.Byte();
			st.sC0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC4 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.sC8 = new byte[156];
			for (int i = 0; i < 156; i++)
				st.sC8[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
			return st;
		}

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;
            buf.Add(GetBytes(ServerName, 64)); // 32 UTF-16 chars = 64 bytes
            buf.Add(s40);
            buf.Add(sC0);
            buf.Add(sC4);
			buf.Add(sC8);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}
