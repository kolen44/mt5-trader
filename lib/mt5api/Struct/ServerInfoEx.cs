using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	/// <summary>
	/// Server information from servers.dat
	/// </summary>
	public class ServerInfo : FromBufReader, ToBufWriter
	{
		internal static readonly int Size = 0x294;
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
		private byte[] s220;

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
        }

        internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + Size;
			var st = new ServerInfo();
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
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): " + buf.CurrentIndex+ " != " + endInd);
			return st;
		}

        public void WriteToBuf(OutBuf buf)
        {
            int countBefore = buf.List.Count;
            buf.Add(GetBytes(ServerName, 128));
            buf.Add(GetBytes(CompanyName, 256));
            buf.Add(s180);
            buf.Add(s184);
            buf.Add(DST);
            buf.Add(TimeZone);
            buf.Add(s190);
            buf.Add(GetBytes(Address, 128));
            buf.Add(PingTime);
            buf.Add(s218);
            buf.Add(s21C);
            buf.Add(s220);
            int written = buf.List.Count - countBefore;
            if (written != Size)
                throw new Exception($"Wrong writing to buffer (Size mismatch): {written} != {Size}");
        }
    }
}
