using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    class PackEncrypt
    {
        byte EncryptByte = 0;
        int EncryptIndex = 0;
        internal byte[] CryptKey;

        internal PackEncrypt(byte[] cryptKey)
        {
            CryptKey = cryptKey;
        }

        internal void EncryptPacket(OutBuf buf)
        {
            for (int i = 9; i < buf.List.Count; i++)
            {
                byte b = buf.List[i];
                buf.List[i] ^= (byte)(EncryptByte + CryptKey[EncryptIndex % CryptKey.Length]);
                EncryptIndex++;
                EncryptByte = b;
            }
        }
    }
}
