using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x6B4, CharSet = CharSet.Unicode)]*/
	/// <summary>
	/// Server information from servers.dat
	/// </summary>
	public class ServerInfoEx : FromBufReader, ToBufWriter
	{
		internal static readonly int Size = 1716;
		/*[FieldOffset(0)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string ServerName;
		/*[FieldOffset(128)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]*/
		public string CompanyName;
		/*[FieldOffset(384)]*/
		int s180;
		/*[FieldOffset(388)]*/
		int s184;
		/*[FieldOffset(392)]*/
		public int DST;
		/*[FieldOffset(396)]*/
		public int TimeZone;
		/*[FieldOffset(400)]*/
		int s190;
		/*[FieldOffset(404)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]*/
		public string Address;
		/*[FieldOffset(532)]*/
		public int PingTime;
		/*[FieldOffset(536)]*/
		int s218;
		/*[FieldOffset(540)]*/
		int s21C;
		/*[FieldOffset(544)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 116)]*/
		byte[] s220 = new byte[116];
		/*[FieldOffset(660)]*/
		int s294;
		/*[FieldOffset(664)]*/
		int s298;
		/*[FieldOffset(668)]*/
		long s29C;
		/*[FieldOffset(676)]*/
		long s2A4;
		/*[FieldOffset(684)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/
		public string CompanyLink;
		/*[FieldOffset(1196)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]*/
		string s4AC;
		/*[FieldOffset(1708)]*/
		long s6AC;

        public void Set(ServerInfoEx source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ServerName = source.ServerName;
            CompanyName = source.CompanyName;
            DST = source.DST;
            TimeZone = source.TimeZone;
            Address = source.Address;
            PingTime = source.PingTime;
            CompanyLink = source.CompanyLink;
        }

        internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 1716;
			var st = new ServerInfoEx();
			st.ServerName = GetString(buf.Bytes(128));
			st.CompanyName = GetString(buf.Bytes(256));
			st.s180 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s184 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.DST = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TimeZone = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s190 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Address = GetString(buf.Bytes(128));
			st.PingTime = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s218 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s21C = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s220 = new byte[116];
			for (int i = 0; i < 116; i++)
				st.s220[i] = buf.Byte();
			st.s294 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s298 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.s29C = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.s2A4 = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.CompanyLink = GetString(buf.Bytes(512));
			st.s4AC = GetString(buf.Bytes(512));
			st.s6AC = BitConverter.ToInt64(buf.Bytes(8), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
			return st;
		}

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;
            buf.Add(GetBytes(ServerName, 128));     // 64 UTF-16 chars = 128 bytes
            buf.Add(GetBytes(CompanyName, 256));    // 128 UTF-16 chars = 256 bytes
            buf.Add(s180);
            buf.Add(s184);
            buf.Add(DST);
            buf.Add(TimeZone);
            buf.Add(s190);
            buf.Add(GetBytes(Address, 128));        // 64 UTF-16 chars = 128 bytes
            buf.Add(PingTime);
            buf.Add(s218);
            buf.Add(s21C);
            buf.Add(s220);
            buf.Add(s294);
            buf.Add(s298);
            buf.Add(s29C);
            buf.Add(s2A4);
            buf.Add(GetBytes(CompanyLink, 512));    // 256 UTF-16 chars = 512 bytes
            buf.Add(GetBytes(s4AC, 512));           // 256 UTF-16 chars = 512 bytes
            buf.Add(s6AC);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}
