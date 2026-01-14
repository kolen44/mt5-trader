using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace mtapi.mt5
{
    class UDT
    {
		//public static T ReadStruct<T>(byte[] data, int offset)
		//{
		//    int size = Marshal.SizeOf(typeof(T));
		//    IntPtr ptr = Marshal.AllocHGlobal(size);
		//    Marshal.Copy(data, offset, ptr, size);
		//    T temp = (T)Marshal.PtrToStructure(ptr, typeof(T));
		//    Marshal.FreeHGlobal(ptr);
		//    return temp;
		//}

		//public static T ReadStruct<T>(byte[] data, int offset, int size)
		//{
		//	IntPtr ptr = Marshal.AllocHGlobal(size);
		//	Marshal.Copy(data, offset, ptr, size);
		//	T temp = (T)Marshal.PtrToStructure(ptr, typeof(T));
		//	Marshal.FreeHGlobal(ptr);
		//	return temp;
		//}

		public static T ReadStruct<T>(InBuf buf) where T : FromBufReader, new()
		{
			var reader = new T();
			return (T)reader.ReadFromBuf(buf);
		}

        public static void WriteStruct<T>(OutBuf buf) where T : ToBufWriter, new()
        {
            var writer = new T();
            writer.WriteToBuf(buf);
        }

        public static T ReadStruct<T>(byte[] data, int offset, int size) where T : FromBufReader, new()
		{
			InBuf buf = new InBuf(data, offset);
			var reader = new T();
			return (T)reader.ReadFromBuf(buf);
		}

		public static byte[] GetBytes(AccountRequest obj)
		{
			var size = 0x58A;
			byte[] buffer = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(obj, ptr, false);
			Marshal.Copy(ptr, buffer, 0, size);
			Marshal.FreeHGlobal(ptr);
			return buffer;
		}

		public static byte[] GetBytes(TradeRequest req)
		{
			OutBuf buf = new OutBuf();
			req.WriteToBuf(buf);
			return buf.List.ToArray();
		}

	}
}
