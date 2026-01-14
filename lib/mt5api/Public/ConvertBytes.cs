using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Convert byte array to strings.
    /// </summary>
    public class ConvertBytes
	{
        /// <summary>
        /// Convert to HEX string.
        /// </summary>
        public static string ToHex(byte[] bytes)
		{
			string res = "";
			foreach (byte b in bytes)
				res += b.ToString("X").PadLeft(2, '0') + " ";
			return res;
		}

        /// <summary>
        /// Convert to ASCII string.
        /// </summary>
        public static string ToAscii(byte[] bytes)
		{
			string res = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == 0)
					res += " ";
				else
					res += (char)bytes[i];
			}
			return res;
		}

        public static string ToUnicode(byte[] bytes)
        {
            //List<byte> list = new List<byte>();
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    if (bytes[i] == 0 && bytes[i+1] ==0)
            //        break;
            //    list.Add(bytes[i]);
            //}
            var str = Encoding.Unicode.GetString(bytes);
            string res = "";
            foreach (var item in str)
            {
                if (item == 0)
                    break;
                res += item;
            }
            return res;
        }
	}

    /// <summary>
    /// Convert string to byte array.
    /// </summary>
    class vUTF
    {
        /// <summary>
        /// Convert UNICODE string to byte array.
        /// </summary>
        public static byte[] toByte(string str)
        {
            byte[] res = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
                res[i] = (byte)(str[i] & 0xFF);
            return res;
        }

        /// <summary>
        /// Convert byte array to UNICODE string.
        /// </summary>
        public static string toString(byte[] bytes, int offset)
        {
            string str = "";
            for (int i = offset; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    break;
                str += (char)bytes[i];
            }
            return str;
        }
    }
}
