using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    class PackDecrypt
    {
        byte DecryptByte = 0;
        int DecryptIndex = 0;
        byte[] CryptKey;

        internal PackDecrypt(byte[] cryptKey)
        {
            CryptKey = cryptKey;
        }

        private readonly SemaphoreSlim DecodeLock = new SemaphoreSlim(1, 1);
        internal async Task<byte[]> Decrypt(byte[] bytes)
        {
            await DecodeLock.WaitAsync();
            try
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= (byte)(DecryptByte + CryptKey[DecryptIndex % CryptKey.Length]);
                    DecryptIndex++;
                    DecryptByte = bytes[i];
                }
                return bytes;
            }
            finally
            {
                DecodeLock.Release();
            }
        }
    }
}
