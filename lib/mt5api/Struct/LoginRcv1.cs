using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Size = 0x2C, CharSet = CharSet.Unicode)]*/
	class LoginRcv1 : FromBufReader
    {
        /*[FieldOffset(0)]*/ private int s0;
        /*[FieldOffset(4)]*/ public Msg StatusCode; //4
        /*[FieldOffset(8)]*/ public int CertType; //8
        /*[FieldOffset(0xC)]*/  long SerialNumber; //C
        /*[FieldOffset(0x14)]*/ public int PassLength; //14
        /*[FieldOffset(0x18)]*/ public short TradeBuild; //18
        /*[FieldOffset(0x1A)]*/ public short SymBuild; //1A
		  /*[FieldOffset(0x1C)]*/  /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ public byte[] CryptKey; //1C
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 44;
			var st = new LoginRcv1();
			st.s0 = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.StatusCode = (Msg)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.CertType = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.SerialNumber = BitConverter.ToInt64(buf.Bytes(8), 0);
			st.PassLength = BitConverter.ToInt32(buf.Bytes(4), 0);
			st.TradeBuild = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.SymBuild = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.CryptKey = new byte[16];
			for (int i = 0; i < 16; i++)
				st.CryptKey[i] = buf.Byte();
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}

}
