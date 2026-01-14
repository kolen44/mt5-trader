using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x94, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Server address from servers.dat
	/// </summary>
	public class AddressRec : FromBufReader, ToBufWriter
	{
		internal static readonly int Size = 148;
		/*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string Address;
        /*[FieldOffset(128)]*/
        internal int s80;
        /*[FieldOffset(132)]*/
        internal int s84;
        /*[FieldOffset(136)]*/	
        internal int s88;
        /*[FieldOffset(140)]*/
        internal int s8C;
        /*[FieldOffset(144)]*/
        internal int s90;

        public AddressRec Clone()
        {
            return new AddressRec
            {
                Address = this.Address,
                s80 = this.s80,
                s84 = this.s84,
                s88 = this.s88,
                s8C = this.s8C,
                s90 = this.s90
            };
        }

        internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 148;
			var st = new AddressRec();
			st.Address = GetString(buf.Bytes(128));
			st.s80 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s84 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s88 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s8C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s90 = BitConverter.ToInt32(buf.Bytes(4), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex + " != " + endInd);
			return st;
		}

		public void WriteToBuf(OutBuf buf)
		{
			int countBefore = buf.List.Count;
			buf.Add(GetBytes(Address, 128)); // 64 UTF-16 chars = 128 bytes
			buf.Add(s80);
			buf.Add(s84);
			buf.Add(s88);
			buf.Add(s8C);
			buf.Add(s90);
			int written = buf.List.Count - countBefore;
			if (written != Size)
				throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
		}
	}
}