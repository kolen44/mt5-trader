using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	internal class Decoder
	{
		private byte Last = 0;
		private int KeyInd = 0;
		private byte[] Key;

		public Decoder(byte[] key)
		{
			Key = key;
		}

        public void ChangeKey(byte[] key)
        {
            Key = key;
        }

        public byte[] GetKey()
        {
            return Key;
        }

        public void Reset()
        {
            Last = 0;
            KeyInd = 0;
        }

        public byte[] Decode(byte[] buf)
		{
			byte[] res = new byte[buf.Length];
			for (int i = 0; i < buf.Length; i++)
			{
				KeyInd &= 0xF;
				res[i] = (byte)(buf[i] ^ (Last + Key[KeyInd]));
				KeyInd++;
				Last = res[i];
			}
			return res;
		}		
	}
}
