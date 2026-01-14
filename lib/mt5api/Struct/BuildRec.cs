using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
	/*[StructLayout(LayoutKind.Explicit, Size = 0x20, CharSet = CharSet.Unicode)]*/
	class BuildRec : FromBufReader //sizeof 0x20 c 
	{
        /*[FieldOffset(0)]*/ public short s0;
        /*[FieldOffset(2)]*/ public Msg StatusCode; //2
        /*[FieldOffset(6)]*/ public short Build; //6
        /*[FieldOffset(8)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/ public byte[] SignData; //8
        /*[FieldOffset(0x18)]*/ /*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]*/ public short[] LastBuild; //0x18
    	internal override object ReadFromBuf(InBuf buf)
		{
			var endInd = buf.CurrentIndex + 32;
			var st = new BuildRec(); //sizeof 0x20 c();
			st.s0 = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.StatusCode = (Msg)BitConverter.ToInt32(buf.Bytes(4), 0);
			st.Build = BitConverter.ToInt16(buf.Bytes(2), 0);
			st.SignData = new byte[16];
			for (int i = 0; i < 16; i++)
				st.SignData[i] = buf.Byte();
			st.LastBuild = new short[4];
			for (int i = 0; i < 4; i++)
				st.LastBuild[i] = BitConverter.ToInt16(buf.Bytes(2), 0);
			if (buf.CurrentIndex != endInd)
				throw new Exception("Wrong reading from buffer(buf.CurrentIndex != endInd): "+buf.CurrentIndex+" != "+endInd);
			return st;
		}
}

	///*[StructLayout(LayoutKind.Explicit, Size = 0x22, CharSet = CharSet.Unicode)]*/
	//public class LoginSnd1 //sizeof 0x22 c : FromBufReader
	//{
	//    /*[FieldOffset(0)]*/ public short s0;
	//    ///*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/
	//    /*[FieldOffset(0x2)]*/ public byte[] CryptKey; //2
	//                                               ///*[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]*/
	//    /*[FieldOffset(0x12)]*/ public byte[] Random; //12
	//}

	//public class vLoginRequest //sizeof 0x22 c : FromBufReader
	//{
	//    public sbyte m_cRandom; //0
	//    public sbyte m_Type; //1
	//    public short m_nRevision; //2
	//    public short m_Signature; //4
	//    public ulong m_nLogin; //6
	//    public byte[] m_Key = new byte[16]; //E
	//    public int m_Random; //1E
	//}

}
