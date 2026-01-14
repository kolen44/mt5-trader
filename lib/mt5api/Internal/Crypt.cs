using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace mtapi.mt5 
{
    internal class Crypt
    {
        public static byte[] CryptKey = new byte[]
		{
			0x41, 0xB6, 0x7F, 0x58, 0x38, 0x0C, 0xF0, 0x2D, 0x7B, 0x39, 0x08, 0xFE, 0x21, 0xBB, 0x41, 0x58
		};

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] EasyCrypt(byte[] buf)
        {
            int last = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] ^= (byte)(last + CryptKey[i & 0xF]);
                last = buf[i];
            }
            return buf;
        }

        public static byte[] EasyDecrypt(byte[] buf)
        {
            int Last = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                int lst = Last;
                Last = buf[i];
                buf[i] ^= (byte)(lst + CryptKey[i & 0xF]);
            }
           return buf;
        }

        public static byte[] Encrypt(byte[] buf)
        {
            int Last = 0;
            byte[] res = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                res[i] = (byte)(buf[i] ^ (Last + CryptKey[i & 0xF]));
                Last = res[i];
            }
            return buf;
        }

        public static byte[] Decrypt(byte[] buf)
        {
            int Last = 0;
            byte[] res = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                res[i] = (byte)(buf[i] ^ (Last + CryptKey[i & 0xF]));
                Last = buf[i];
            }
            return res;
        }

        public static byte[] Encode(byte[] buf, byte[] key)
        {
            int Last = 0;
            byte[] res = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                res[i] = (byte)(buf[i] ^ (Last + key[i % key.Length]));
                Last = res[i];
            }
            return res;
        }

        public static byte[] Decode(byte[] buf, byte[] key)
        {
            int Last = 0;
            byte[] res = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                res[i] = (byte)(buf[i] ^ (Last + key[i % key.Length]));
                Last = buf[i];
            }
            return res;
        }

        static byte[] _HardId = new byte[16];

        private static void CreateHardId()
        {
            uint seed = (uint)DateTime.Now.Ticks;//522441350;//
            byte[] data = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                seed = seed * 214013 + 2531011;
                data[i] = (byte)((seed >> 16) & 0xFF);
            }
            MD5 md = new MD5CryptoServiceProvider();
            _HardId = md.ComputeHash(data);
            _HardId[0] = 0;
            for (int i = 1; i < 16; i++)
                _HardId[0] += _HardId[i];
        }

        public static byte[] GetHardId()
        {
            lock(_HardId)
            if (_HardId[0] == 0 || _HardId[15] == 0)
                CreateHardId();
            return _HardId;
        }
    }

    internal class vSHA1
    {
        private uint[] Regs = new uint[5];
        private int nBitCount = 0;
        private uint dwData = 0;
        private byte[] dwBlock = new byte[64];
        private int dwCount = 0;
        private int dbCount = 0;

        public vSHA1()
        {
            nBitCount = 0;
            dwData = 0;
            dwCount = 0;
            dbCount = 0;
            Regs[0] = 0x67452301;
            Regs[1] = 0xEFCDAB89;
            Regs[2] = 0x98BADCFE;
            Regs[3] = 0x10325476;
            Regs[4] = 0xC3D2E1F0;
            Array.Clear(dwBlock, 0, 16);
        }

        public void HashData(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                dwData = (dwData << 8) + data[i];
                nBitCount += 8;
                if (++dbCount >= 4)
                {
                    dbCount = 0;
                    BitConverter.GetBytes(dwData).CopyTo(dwBlock, dwCount * 4);
                    if (++dwCount >= 16)
                    {
                        dwCount = 0;
                        Transform(dwBlock);
                    }
                    dwData = 0;
                }
            }
        }

        public byte[] FinalizeHash()
        {
            int bitCnt = nBitCount;
            dwData = (dwData << 8) + 0x80;
            while (true)
            {
                nBitCount += 8;
                if (++dbCount >= 4)
                {
                    dbCount = 0;
                    BitConverter.GetBytes(dwData).CopyTo(dwBlock, dwCount * 4);
                    if (++dwCount >= 16)
                    {
                        dwCount = 0;
                        Transform(dwBlock);
                    }
                    dwData = 0;
                }
                if ((dbCount == 0) && (dwCount == 14))
                    break;
                dwData <<= 8;
            }
            BitConverter.GetBytes(0).CopyTo(dwBlock, dwCount * 4);
            if (++dwCount >= 16)
            {
                dwCount = 0;
                Transform(dwBlock);
            }
            BitConverter.GetBytes(bitCnt).CopyTo(dwBlock, dwCount * 4);
            if (++dwCount >= 16)
            {
                dwCount = 0;
                Transform(dwBlock);
            }
            return new byte[]
			{
			   (byte)(Regs[0] >> 24),
			   (byte)(Regs[0] >> 16),
			   (byte)(Regs[0] >> 8),
			   (byte)(Regs[0] >> 0),
			   (byte)(Regs[1] >> 24),
			   (byte)(Regs[1] >> 16),
			   (byte)(Regs[1] >> 8),
			   (byte)(Regs[1] >> 0),
			   (byte)(Regs[2] >> 24),
			   (byte)(Regs[2] >> 16),
			   (byte)(Regs[2] >> 8),
			   (byte)(Regs[2] >> 0),
			   (byte)(Regs[3] >> 24),
			   (byte)(Regs[3] >> 16),
			   (byte)(Regs[3] >> 8),
			   (byte)(Regs[3] >> 0),
			   (byte)(Regs[4] >> 24),
			   (byte)(Regs[4] >> 16),
			   (byte)(Regs[4] >> 8),
			   (byte)(Regs[4] >> 0),
			};
        }

        public byte[] ComputeHash(byte[] data)
        {
            int len = data.Length;
            int left = 0;
            if (len >= 64)
            {
                byte[] block = new byte[64];
                for (int i = 0; i < len / 64; i++)
                {
                    for (int k = 0; k < 64; k++)
                        block[k] = data[i * 64 + k];
                    Transform(block);
                    left += 64;
                }
            }
            int rem = len % 64;
            if (rem > 0)
            {
                byte[] block = new byte[64];
                for (int k = 0; k < rem; k++)
                    block[k] = data[left + k];
                Transform(block);
            }
            return new byte[]
			{
			   (byte)(Regs[0] >> 0),
			   (byte)(Regs[0] >> 8),
			   (byte)(Regs[0] >> 16),
			   (byte)(Regs[0] >> 24),
			   (byte)(Regs[1] >> 0),
			   (byte)(Regs[1] >> 8),
			   (byte)(Regs[1] >> 16),
			   (byte)(Regs[1] >> 24),
			   (byte)(Regs[2] >> 0),
			   (byte)(Regs[2] >> 8),
			   (byte)(Regs[2] >> 16),
			   (byte)(Regs[2] >> 24),
			   (byte)(Regs[3] >> 0),
			   (byte)(Regs[3] >> 8),
			   (byte)(Regs[3] >> 16),
			   (byte)(Regs[3] >> 24),
			   (byte)(Regs[4] >> 0),
			   (byte)(Regs[4] >> 8),
			   (byte)(Regs[4] >> 16),
			   (byte)(Regs[4] >> 24),
			};
        }

        private uint SHA1Shift(int bits, uint word)
        {
            return ((word << bits) | (word >> (32 - bits)));
        }

        private void Transform(byte[] data)
        {
            uint temp;
            uint[] W = new uint[80];
            for(int i = 0; i < 16; i++)
                W[i] = BitConverter.ToUInt32(data, i * 4);
            for (int i = 16; i < 80; i++)
                W[i] = SHA1Shift(1, W[i-3] ^ W[i-8] ^ W[i-14] ^ W[i-16]);
            uint A = Regs[0];
            uint B = Regs[1];
            uint C = Regs[2];
            uint D = Regs[3];
            uint E = Regs[4];
            for (int i = 0; i < 20; i++)
            {
                temp = SHA1Shift(5, A) + ((B & C) | (~B & D)) + E + W[i] + 0x5A827999;
                E = D; D = C;
                C = SHA1Shift(30, B);
                B = A; A = temp;
            }
            for (int i = 20; i < 40; i++)
            {
                temp = SHA1Shift(5, A) + (B ^ C ^ D) + E + W[i] + 0x6ED9EBA1;
                E = D; D = C;
                C = SHA1Shift(30, B);
                B = A; A = temp;
            }
            for (int i = 40; i < 60; i++)
            {
                temp = SHA1Shift(5, A) + ((B & C) | (B & D) | (C & D)) + E + W[i] + 0x8F1BBCDC;
                E = D; D = C;
                C = SHA1Shift(30, B);
                B = A; A = temp;
            }
            for (int i = 60; i < 80; i++)
            {
                temp = SHA1Shift(5, A) + (B ^ C ^ D) + E + W[i] + 0xCA62C1D6;
                E = D; D = C;
                C = SHA1Shift(30, B);
                B = A; A = temp;
            }
            Regs[0] += A;
            Regs[1] += B;
            Regs[2] += C;
            Regs[3] += D;
            Regs[4] += E;
        }
    }

    internal class vRSA
    {
        private ulong P, Q, M, N, D, /*E, */Y;

        public vRSA(ulong p)
        {
            P = p ^ 0x151D8255;
            Q = p ^ 0x274ECC00;
            M = 0x67789ED4559AF79;
            N = 0xCCCCCCCCCCCCCCCC;
            D = 0x1DE7FED38081;
            //E = 0x5D405B5;
            Y = 0x53290744C4D541;
        }

        public ulong ComputePacketKey(byte[] data)
        {
            return ExpMod64(PrepareKey(data) % N, N, M);
        }

        public ulong ComputeFileKey(byte[] data)
        {
            return ExpMod64(PrepareKey(data) % M, D, M);
        }

        public bool CheckKey(byte[] data)
        {
            if (data.Length < 8)
                return false;
            int szData = data.Length - 8;
            byte[] buf = new byte[szData];
            Array.Copy(data, buf, szData);
            ulong dataKey = PrepareKey(buf);
            ulong origKey = BitConverter.ToUInt64(data, szData);
            return ExpMod64(origKey, Y, M) == (dataKey % M);
        }

        private ulong PrepareKey(byte[] data)
        {
            if (data.Length < 1)
                return 0;
            ulong h = 0;
            ulong pm = 0x123456789;
            for (int i = 0; i < data.Length / 8; i++)
            {
                ulong w = BitConverter.ToUInt64(data, i * 8);
                h ^= w;
                uint lw = (uint)w;
                uint hw = (uint)(w >> 32);
                ulong sign = ((hw & 0x80000000) != 0) ? 0xFFFFFFFF00000000 : 0;
                ulong t = ((ulong)(hw ^ lw) << 32) | (ulong)(lw ^ hw);
                t += sign;
                t += (((ulong)lw << 32) | hw) | sign;
                t += pm;
                t += w;
                pm ^= t;
            }
            int rem = data.Length & 7;
            if (rem != 0)
            {
                long ls = 0;
                long rs = 0;
                long mdc = 0x11F71FB04CB;
                int last = 0;
                if (rem >= 2)
                {
                    int it = (rem - 2) / 2 + 1;
                    last = it * 2;
                    int off = data.Length - rem;
                    int sh = 0;
                    for (int i = 0; i < it; i++, sh += 16)
                    {
                        ls += (long)((sbyte)data[i * 2 + off]) << sh;
                        rs += (long)((sbyte)data[i * 2 + 1 + off]) << (sh + 8);
                    }
                }
                if (last < rem)
                    mdc += (long)((sbyte)data[data.Length - 1]) << (last << 8);
                ulong w = (ulong)(ls + rs + mdc);
                h ^= w;
                uint lw = (uint)w;
                uint hw = (uint)(w >> 32);
                ulong sign = ((hw & 0x80000000) != 0) ? 0xFFFFFFFF00000000 : 0;
                ulong t = ((ulong)(hw ^ lw) << 32) | (ulong)(lw ^ hw);
                t += sign;
                t += (((ulong)lw << 32) | hw) | sign;
                t += pm;
                t += w;
                pm ^= t;
            }
            return (((ulong)data.Length * 0x100000001) ^ pm ^ h) & 0xFFFFFFFFFFF;
        }

        private ulong ExpMod64(ulong rem, ulong n, ulong m)
        {
            ulong key = 1;
            ulong prv = rem;
            for (int i = 0; i < 64; i++)
            {
                if (((n >> i) & 1) != 0)
                    key = MulMod64(key, prv, m);
                prv = MulMod64(prv, prv, m);
            }
            return key;
        }

        private ulong MulMod64(ulong k, ulong n, ulong m)
        {
            ulong key = 0;
            ulong prv = k;
            for (int i = 0; i < 64; i++)
            {
                if (((n >> i) & 1) != 0)
                    key = (key + prv) % m;
                prv = prv * 2 % m;
            }
            return key;
        }
    }

    internal class vAES
    {
	    private int	m_nCipherRnd;
        private uint[] m_Ks = new uint[64];             //KeySchedule
        private uint[] m_Ke = new uint[64];             //KeyEncoded
        private uint[,] s_tabIT = new uint[4, 256];
	    private uint[,] s_tabFT = new uint[4, 256];
	    private uint[] s_tabIB = new uint[256];
	    private uint[] s_tabSB = new uint[256];

        public vAES()
        {
	        m_nCipherRnd = 0;
            Array.Clear(m_Ks, 0, 64);
            Array.Clear(m_Ke, 0, 64);
        }

		private byte[] EncryptBlock(byte[] data)
		{
			uint[] ind = new uint[4];
			uint[] w = new uint[4];
			ind[0] = m_Ks[0] ^ BitConverter.ToUInt32(data, 0);
			ind[1] = m_Ks[1] ^ BitConverter.ToUInt32(data, 4);
			ind[2] = m_Ks[2] ^ BitConverter.ToUInt32(data, 8);
			ind[3] = m_Ks[3] ^ BitConverter.ToUInt32(data, 12);
			w[0] = s_tabFT[0, ind[0] & 0xFF] ^ s_tabFT[1, (ind[1] >> 8) & 0xFF] ^
				s_tabFT[2, (ind[2] >> 16) & 0xFF] ^ s_tabFT[3, (ind[3] >> 24) & 0xFF] ^ m_Ks[4];
			w[1] = s_tabFT[0, ind[1] & 0xFF] ^ s_tabFT[1, (ind[2] >> 8) & 0xFF] ^
				s_tabFT[2, (ind[3] >> 16) & 0xFF] ^ s_tabFT[3, (ind[0] >> 24) & 0xFF] ^ m_Ks[5];
			w[2] = s_tabFT[0, ind[2] & 0xFF] ^ s_tabFT[1, (ind[3] >> 8) & 0xFF] ^
				s_tabFT[2, (ind[0] >> 16) & 0xFF] ^ s_tabFT[3, (ind[1] >> 24) & 0xFF] ^ m_Ks[6];
			w[3] = s_tabFT[0, ind[3] & 0xFF] ^ s_tabFT[1, (ind[0] >> 8) & 0xFF] ^
				s_tabFT[2, (ind[1] >> 16) & 0xFF] ^ s_tabFT[3, (ind[2] >> 24) & 0xFF] ^ m_Ks[7];
			w.CopyTo(ind, 0);
			int i;
			for (i = 0; i < m_nCipherRnd - 2; i += 2)
			{
				w[0] = s_tabFT[0, ind[0] & 0xFF] ^ s_tabFT[1, (ind[1] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[2] >> 16) & 0xFF] ^ s_tabFT[3, (ind[3] >> 24) & 0xFF] ^ m_Ks[i * 4 + 8];
				w[1] = s_tabFT[0, ind[1] & 0xFF] ^ s_tabFT[1, (ind[2] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[3] >> 16) & 0xFF] ^ s_tabFT[3, (ind[0] >> 24) & 0xFF] ^ m_Ks[i * 4 + 9];
				w[2] = s_tabFT[0, ind[2] & 0xFF] ^ s_tabFT[1, (ind[3] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[0] >> 16) & 0xFF] ^ s_tabFT[3, (ind[1] >> 24) & 0xFF] ^ m_Ks[i * 4 + 10];
				w[3] = s_tabFT[0, ind[3] & 0xFF] ^ s_tabFT[1, (ind[0] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[1] >> 16) & 0xFF] ^ s_tabFT[3, (ind[2] >> 24) & 0xFF] ^ m_Ks[i * 4 + 11];
				w.CopyTo(ind, 0);
				w[0] = s_tabFT[0, ind[0] & 0xFF] ^ s_tabFT[1, (ind[1] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[2] >> 16) & 0xFF] ^ s_tabFT[3, (ind[3] >> 24) & 0xFF] ^ m_Ks[i * 4 + 12];
				w[1] = s_tabFT[0, ind[1] & 0xFF] ^ s_tabFT[1, (ind[2] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[3] >> 16) & 0xFF] ^ s_tabFT[3, (ind[0] >> 24) & 0xFF] ^ m_Ks[i * 4 + 13];
				w[2] = s_tabFT[0, ind[2] & 0xFF] ^ s_tabFT[1, (ind[3] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[0] >> 16) & 0xFF] ^ s_tabFT[3, (ind[1] >> 24) & 0xFF] ^ m_Ks[i * 4 + 14];
				w[3] = s_tabFT[0, ind[3] & 0xFF] ^ s_tabFT[1, (ind[0] >> 8) & 0xFF] ^
					s_tabFT[2, (ind[1] >> 16) & 0xFF] ^ s_tabFT[3, (ind[2] >> 24) & 0xFF] ^ m_Ks[i * 4 + 15];
				w.CopyTo(ind, 0);
			}
			byte[] crypt = new byte[16];
			BitConverter.GetBytes(s_tabSB[ind[0] & 0xFF] ^ (s_tabSB[(ind[1] >> 8) & 0xFF] << 8) ^
				(s_tabSB[(ind[2] >> 16) & 0xFF] << 16) ^ (s_tabSB[(ind[3] >> 24) & 0xFF] << 24) ^ m_Ks[i * 4 + 8]).CopyTo(crypt, 0);
			BitConverter.GetBytes(s_tabSB[ind[1] & 0xFF] ^ (s_tabSB[(ind[2] >> 8) & 0xFF] << 8) ^
				(s_tabSB[(ind[3] >> 16) & 0xFF] << 16) ^ (s_tabSB[(ind[0] >> 24) & 0xFF] << 24) ^ m_Ks[i * 4 + 9]).CopyTo(crypt, 4);
			BitConverter.GetBytes(s_tabSB[ind[2] & 0xFF] ^ (s_tabSB[(ind[3] >> 8) & 0xFF] << 8) ^
				(s_tabSB[(ind[0] >> 16) & 0xFF] << 16) ^ (s_tabSB[(ind[1] >> 24) & 0xFF] << 24) ^ m_Ks[i * 4 + 10]).CopyTo(crypt, 8);
			BitConverter.GetBytes(s_tabSB[ind[3] & 0xFF] ^ (s_tabSB[(ind[0] >> 8) & 0xFF] << 8) ^
				(s_tabSB[(ind[1] >> 16) & 0xFF] << 16) ^ (s_tabSB[(ind[2] >> 24) & 0xFF] << 24) ^ m_Ks[i * 4 + 11]).CopyTo(crypt, 12);
			return crypt;
		}

		private byte[] DecryptBlock(byte[] data)
		{
			uint[] ind = new uint[4];
			uint[] w = new uint[4];
			ind[0] = m_Ke[0] ^ BitConverter.ToUInt32(data, 0);
			ind[1] = m_Ke[1] ^ BitConverter.ToUInt32(data, 4);
			ind[2] = m_Ke[2] ^ BitConverter.ToUInt32(data, 8);
			ind[3] = m_Ke[3] ^ BitConverter.ToUInt32(data, 12);
			w[0] = s_tabIT[0, ind[0] & 0xFF] ^ s_tabIT[1, (ind[3] >> 8) & 0xFF] ^
				s_tabIT[2, (ind[2] >> 16) & 0xFF] ^ s_tabIT[3, (ind[1] >> 24) & 0xFF] ^ m_Ke[4];
			w[1] = s_tabIT[0, ind[1] & 0xFF] ^ s_tabIT[1, (ind[0] >> 8) & 0xFF] ^
				s_tabIT[2, (ind[3] >> 16) & 0xFF] ^ s_tabIT[3, (ind[2] >> 24) & 0xFF] ^ m_Ke[5];
			w[2] = s_tabIT[0, ind[2] & 0xFF] ^ s_tabIT[1, (ind[1] >> 8) & 0xFF] ^
				s_tabIT[2, (ind[0] >> 16) & 0xFF] ^ s_tabIT[3, (ind[3] >> 24) & 0xFF] ^ m_Ke[6];
			w[3] = s_tabIT[0, ind[3] & 0xFF] ^ s_tabIT[1, (ind[2] >> 8) & 0xFF] ^
				s_tabIT[2, (ind[1] >> 16) & 0xFF] ^ s_tabIT[3, (ind[0] >> 24) & 0xFF] ^ m_Ke[7];
			w.CopyTo(ind, 0);
			int i;
			for (i = 0; i < m_nCipherRnd - 2; i += 2)
			{
				w[0] = s_tabIT[0, ind[0] & 0xFF] ^ s_tabIT[1, (ind[3] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[2] >> 16) & 0xFF] ^ s_tabIT[3, (ind[1] >> 24) & 0xFF] ^ m_Ke[i * 4 + 8];
				w[1] = s_tabIT[0, ind[1] & 0xFF] ^ s_tabIT[1, (ind[0] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[3] >> 16) & 0xFF] ^ s_tabIT[3, (ind[2] >> 24) & 0xFF] ^ m_Ke[i * 4 + 9];
				w[2] = s_tabIT[0, ind[2] & 0xFF] ^ s_tabIT[1, (ind[1] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[0] >> 16) & 0xFF] ^ s_tabIT[3, (ind[3] >> 24) & 0xFF] ^ m_Ke[i * 4 + 10];
				w[3] = s_tabIT[0, ind[3] & 0xFF] ^ s_tabIT[1, (ind[2] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[1] >> 16) & 0xFF] ^ s_tabIT[3, (ind[0] >> 24) & 0xFF] ^ m_Ke[i * 4 + 11];
				w.CopyTo(ind, 0);
				w[0] = s_tabIT[0, ind[0] & 0xFF] ^ s_tabIT[1, (ind[3] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[2] >> 16) & 0xFF] ^ s_tabIT[3, (ind[1] >> 24) & 0xFF] ^ m_Ke[i * 4 + 12];
				w[1] = s_tabIT[0, ind[1] & 0xFF] ^ s_tabIT[1, (ind[0] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[3] >> 16) & 0xFF] ^ s_tabIT[3, (ind[2] >> 24) & 0xFF] ^ m_Ke[i * 4 + 13];
				w[2] = s_tabIT[0, ind[2] & 0xFF] ^ s_tabIT[1, (ind[1] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[0] >> 16) & 0xFF] ^ s_tabIT[3, (ind[3] >> 24) & 0xFF] ^ m_Ke[i * 4 + 14];
				w[3] = s_tabIT[0, ind[3] & 0xFF] ^ s_tabIT[1, (ind[2] >> 8) & 0xFF] ^
					s_tabIT[2, (ind[1] >> 16) & 0xFF] ^ s_tabIT[3, (ind[0] >> 24) & 0xFF] ^ m_Ke[i * 4 + 15];
				w.CopyTo(ind, 0);
			}
			byte[] crypt = new byte[16];
			BitConverter.GetBytes(s_tabIB[ind[0] & 0xFF] ^ (s_tabIB[(ind[3] >> 8) & 0xFF] << 8) ^
				(s_tabIB[(ind[2] >> 16) & 0xFF] << 16) ^ (s_tabIB[(ind[1] >> 24) & 0xFF] << 24) ^ m_Ke[i * 4 + 8]).CopyTo(crypt, 0);
			BitConverter.GetBytes(s_tabIB[ind[1] & 0xFF] ^ (s_tabIB[(ind[0] >> 8) & 0xFF] << 8) ^
				(s_tabIB[(ind[3] >> 16) & 0xFF] << 16) ^ (s_tabIB[(ind[2] >> 24) & 0xFF] << 24) ^ m_Ke[i * 4 + 9]).CopyTo(crypt, 4);
			BitConverter.GetBytes(s_tabIB[ind[2] & 0xFF] ^ (s_tabIB[(ind[1] >> 8) & 0xFF] << 8) ^
				(s_tabIB[(ind[0] >> 16) & 0xFF] << 16) ^ (s_tabIB[(ind[3] >> 24) & 0xFF] << 24) ^ m_Ke[i * 4 + 10]).CopyTo(crypt, 8);
			BitConverter.GetBytes(s_tabIB[ind[3] & 0xFF] ^ (s_tabIB[(ind[2] >> 8) & 0xFF] << 8) ^
				(s_tabIB[(ind[1] >> 16) & 0xFF] << 16) ^ (s_tabIB[(ind[0] >> 24) & 0xFF] << 24) ^ m_Ke[i * 4 + 11]).CopyTo(crypt, 12);
			return crypt;
		}

		private uint upr(uint x)
        {
            return (x << 8) | (x >> (32 - 8));
        }

        public void GenerateTables()
        {
	        byte[] log = new byte[256];
	        byte[] pow = new byte[256];
	        log[0] = 0;
	        byte w = 1;
	        for(int i = 0; i < 256; i++)
	        {
		        uint v = w;
		        log[v] = (byte)i;
		        pow[i] = w;
		        w ^= (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
	        }
	        pow[255] = 0;
	        for(int i = 0; i < 256; i++)
	        {
		        byte v = pow[255 - log[i]];
		        w = (byte)((((((((v >> 1) ^ v) >> 1) ^ v) >> 1) ^ v) >> 4) ^
			        (((((((v << 1) ^ v) << 1) ^ v) << 1) ^ v) << 1) ^ v ^ 0x63);
		        s_tabSB[i] = w;
		        s_tabIB[w] = (uint)i;
	        }
	        for(int i = 0; i < 256; i++)
	        {
		        byte v1 = (byte)s_tabSB[i];
		        byte v2 = (byte)(v1 << 1);
		        if ((v1 & 0x80) != 0)
			        v2 ^= 0x1B;
		        uint wt = (uint)(((v1 ^ v2) << 24) | (v1 << 16) | (v1 << 8) | v2);
		        s_tabFT[0, i] = wt;
		        wt = upr(wt);
		        s_tabFT[1, i] = wt;
		        wt = upr(wt);
		        s_tabFT[2, i] = wt;
		        wt = upr(wt);
		        s_tabFT[3, i] = wt;
		        wt = 0;
		        byte v = (byte)s_tabIB[i];
		        if(v != 0)
			        wt = (uint)((pow[(log[v] + 0x68) % 255] << 24) ^ (pow[(log[v] + 0xEE) % 255] << 16) ^
				        (pow[(log[v] + 0xC7) % 255] << 8) ^ pow[(log[v] + 0xDF) % 255]);
		        s_tabIT[0, i] = wt;
		        wt = upr(wt);
		        s_tabIT[1, i] = wt;
		        wt = upr(wt);
		        s_tabIT[2, i] = wt;
		        wt = upr(wt);
		        s_tabIT[3, i] = wt;
	        }
        }

        private uint bKs(int index)
        {
            uint ks = m_Ks[index / 4];
            switch (index % 4)
            {
                case 0:
                    return ks & 0xFF;
                case 1:
                    return (ks >> 8) & 0xFF;
                case 2:
                    return (ks >> 16) & 0xFF;
                case 3:
                    return (ks >> 24) & 0xFF;
            }
            return 0;
        }

        public void EncodeKey(byte[] key, int szKey)
        {
	        if(szKey > 256)
		        return;
	        if(s_tabSB[0] == 0)
		        GenerateTables();
            for (int i = 0; i < szKey / 32; i++)
		        m_Ks[i] = BitConverter.ToUInt32(key, i * 4);
	        uint v;
	        byte w = 1;
            int indKs;
	        if(szKey == 128)
	        {
		        for(int i = 0; i < 2; i++)
		        {
                    m_Ks[i * 20 + 4] = (((((s_tabSB[bKs(i * 80 + 12)] << 8) ^ s_tabSB[bKs(i * 80 + 15)]) << 8) ^
                        s_tabSB[bKs(i * 80 + 14)]) << 8) ^ s_tabSB[bKs(i * 80 + 13)] ^ m_Ks[i * 20] ^ w;
                    m_Ks[i * 20 + 5] = m_Ks[i * 20 + 1] ^ m_Ks[i * 20 + 4];
                    m_Ks[i * 20 + 6] = m_Ks[i * 20 + 1] ^ m_Ks[i * 20 + 2] ^ m_Ks[i * 20 + 4];
                    m_Ks[i * 20 + 7] = m_Ks[i * 20 + 3] ^ m_Ks[i * 20 + 6];
			        v = w;
			        w = (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
                    m_Ks[i * 20 + 8] = (((((s_tabSB[bKs(i * 80 + 28)] << 8) ^ s_tabSB[bKs(i * 80 + 31)]) << 8) ^
                        s_tabSB[bKs(i * 80 + 30)]) << 8) ^ s_tabSB[bKs(i * 80 + 29)] ^ m_Ks[i * 20 + 4] ^ w;
                    m_Ks[i * 20 + 9] = m_Ks[i * 20 + 5] ^ m_Ks[i * 20 + 8];
                    m_Ks[i * 20 + 10] = m_Ks[i * 20 + 5] ^ m_Ks[i * 20 + 6] ^ m_Ks[i * 20 + 8];
                    m_Ks[i * 20 + 11] = m_Ks[i * 20 + 7] ^ m_Ks[i * 20 + 10];
			        v = w;
			        w = (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
                    m_Ks[i * 20 + 12] = (((((s_tabSB[bKs(i * 80 + 44)] << 8) ^ s_tabSB[bKs(i * 80 + 47)]) << 8) ^
                        s_tabSB[bKs(i * 80 + 46)]) << 8) ^ s_tabSB[bKs(i * 80 + 45)] ^ m_Ks[i * 20 + 8] ^ w;
                    m_Ks[i * 20 + 13] = m_Ks[i * 20 + 9] ^ m_Ks[i * 20 + 12];
                    m_Ks[i * 20 + 14] = m_Ks[i * 20 + 9] ^ m_Ks[i * 20 + 10] ^ m_Ks[i * 20 + 12];
                    m_Ks[i * 20 + 15] = m_Ks[i * 20 + 11] ^ m_Ks[i * 20 + 14];
			        v = w;
			        w = (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
                    m_Ks[i * 20 + 16] = (((((s_tabSB[bKs(i * 80 + 60)] << 8) ^ s_tabSB[bKs(i * 80 + 63)]) << 8) ^
                        s_tabSB[bKs(i * 80 + 62)]) << 8) ^ s_tabSB[bKs(i * 80 + 61)] ^ m_Ks[i * 20 + 12] ^ w;
                    m_Ks[i * 20 + 17] = m_Ks[i * 20 + 13] ^ m_Ks[i * 20 + 16];
                    m_Ks[i * 20 + 18] = m_Ks[i * 20 + 13] ^ m_Ks[i * 20 + 14] ^ m_Ks[i * 20 + 16];
                    m_Ks[i * 20 + 19] = m_Ks[i * 20 + 15] ^ m_Ks[i * 20 + 18];
			        v = w;
			        w = (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
                    m_Ks[i * 20 + 20] = (((((s_tabSB[bKs(i * 80 + 76)] << 8) ^ s_tabSB[bKs(i * 80 + 79)]) << 8) ^
                        s_tabSB[bKs(i * 80 + 78)]) << 8) ^ s_tabSB[bKs(i * 80 + 77)] ^ m_Ks[i * 20 + 16] ^ w;
                    m_Ks[i * 20 + 21] = m_Ks[i * 20 + 17] ^ m_Ks[i * 20 + 20];
                    m_Ks[i * 20 + 22] = m_Ks[i * 20 + 17] ^ m_Ks[i * 20 + 18] ^ m_Ks[i * 20 + 20];
                    m_Ks[i * 20 + 23] = m_Ks[i * 20 + 19] ^ m_Ks[i * 20 + 22];
			        v = w;
			        w = (byte)((v << 1) ^ (((w & 0x80) != 0) ? 0x1B : 0));
		        }
		        m_nCipherRnd = 10;
                indKs = 80 * 2;
	        }
	        else if(szKey == 196)
	        {
		        for(int i = 0; i < 2; i++)
		        {
                    m_Ks[i * 24 + 6] = (((((s_tabSB[bKs(i * 96 + 20)] << 8) ^ s_tabSB[bKs(i * 96 + 23)]) << 8) ^
                        s_tabSB[bKs(i * 96 + 22)]) << 8) ^ s_tabSB[bKs(i * 96 + 21)] ^ m_Ks[i * 24] ^ w;
                    m_Ks[i * 24 + 7] = m_Ks[i * 24 + 1] ^ m_Ks[i * 24 + 6];
                    m_Ks[i * 24 + 8] = m_Ks[i * 24 + 1] ^ m_Ks[i * 24 + 2] ^ m_Ks[i * 24 + 6];
                    m_Ks[i * 24 + 9] = m_Ks[i * 24 + 3] ^ m_Ks[i * 24 + 8];
                    m_Ks[i * 24 + 10] = m_Ks[i * 24 + 3] ^ m_Ks[i * 24 + 4] ^ m_Ks[i * 24 + 8];
                    m_Ks[i * 24 + 11] = m_Ks[i * 24 + 5] ^ m_Ks[i * 24 + 10];
			        w <<= 1;
                    m_Ks[i * 24 + 12] = (((((s_tabSB[bKs(i * 96 + 44)] << 8) ^ s_tabSB[bKs(i * 96 + 47)]) << 8) ^
                        s_tabSB[bKs(i * 96 + 46)]) << 8) ^ s_tabSB[bKs(i * 96 + 45)] ^ m_Ks[i * 24 + 6] ^ w;
                    m_Ks[i * 24 + 13] = m_Ks[i * 24 + 7] ^ m_Ks[i * 24 + 12];
                    m_Ks[i * 24 + 14] = m_Ks[i * 24 + 7] ^ m_Ks[i * 24 + 8] ^ m_Ks[i * 24 + 12];
                    m_Ks[i * 24 + 15] = m_Ks[i * 24 + 9] ^ m_Ks[i * 24 + 14];
                    m_Ks[i * 24 + 16] = m_Ks[i * 24 + 9] ^ m_Ks[i * 24 + 10] ^ m_Ks[i * 24 + 14];
                    m_Ks[i * 24 + 17] = m_Ks[i * 24 + 11] ^ m_Ks[i * 24 + 16];
			        w <<= 1;
                    m_Ks[i * 24 + 18] = (((((s_tabSB[bKs(i * 96 + 68)] << 8) ^ s_tabSB[bKs(i * 96 + 71)]) << 8) ^
                        s_tabSB[bKs(i * 96 + 70)]) << 8) ^ s_tabSB[bKs(i * 96 + 69)] ^ m_Ks[i * 24 + 12] ^ w;
                    m_Ks[i * 24 + 19] = m_Ks[i * 24 + 13] ^ m_Ks[i * 24 + 18];
                    m_Ks[i * 24 + 20] = m_Ks[i * 24 + 13] ^ m_Ks[i * 24 + 14] ^ m_Ks[i * 24 + 18];
                    m_Ks[i * 24 + 21] = m_Ks[i * 24 + 15] ^ m_Ks[i * 24 + 20];
                    m_Ks[i * 24 + 22] = m_Ks[i * 24 + 15] ^ m_Ks[i * 24 + 16] ^ m_Ks[i * 24 + 20];
                    m_Ks[i * 24 + 23] = m_Ks[i * 24 + 17] ^ m_Ks[i * 24 + 22];
			        w <<= 1;
                    m_Ks[i * 24 + 24] = (((((s_tabSB[bKs(i * 96 + 92)] << 8) ^ s_tabSB[bKs(i * 96 + 95)]) << 8) ^
                        s_tabSB[bKs(i * 96 + 94)]) << 8) ^ s_tabSB[bKs(i * 96 + 93)] ^ m_Ks[i * 24 + 18] ^ w;
                    m_Ks[i * 24 + 25] = m_Ks[i * 24 + 19] ^ m_Ks[i * 24 + 18];
                    m_Ks[i * 24 + 26] = m_Ks[i * 24 + 19] ^ m_Ks[i * 24 + 20] ^ m_Ks[i * 24 + 18];
                    m_Ks[i * 24 + 27] = m_Ks[i * 24 + 21] ^ m_Ks[i * 24 + 26];
                    m_Ks[i * 24 + 28] = m_Ks[i * 24 + 21] ^ m_Ks[i * 24 + 22] ^ m_Ks[i * 24 + 26];
                    m_Ks[i * 24 + 29] = m_Ks[i * 24 + 23] ^ m_Ks[i * 24 + 28];
			        w <<= 1;
		        }
		        m_nCipherRnd = 12;
                indKs = 96 * 2;
	        }
	        else if(szKey == 256)
	        {
		        for(int i = 0; i < 7; i++)
		        {
                    m_Ks[i * 8 + 8] = (((((s_tabSB[bKs(i * 32 + 28)] << 8) ^ s_tabSB[bKs(i * 32 + 31)]) << 8) ^
                        s_tabSB[bKs(i * 32 + 30)]) << 8) ^ s_tabSB[bKs(i * 32 + 29)] ^ m_Ks[i * 8] ^ w;
                    m_Ks[i * 8 + 9] = m_Ks[i * 8 + 1] ^ m_Ks[i * 8 + 8];
                    m_Ks[i * 8 + 10] = m_Ks[i * 8 + 1] ^ m_Ks[i * 8 + 2] ^ m_Ks[i * 8 + 8];
                    m_Ks[i * 8 + 11] = m_Ks[i * 8 + 3] ^ m_Ks[i * 8 + 10];
			        w <<= 1;
                    m_Ks[i * 8 + 12] = (((((s_tabSB[bKs(i * 32 + 44)] << 8) ^ s_tabSB[bKs(i * 32 + 47)]) << 8) ^
                        s_tabSB[bKs(i * 32 + 46)]) << 8) ^ s_tabSB[bKs(i * 32 + 45)] ^ m_Ks[i * 8 + 4];
                    m_Ks[i * 8 + 13] = m_Ks[i * 8 + 5] ^ m_Ks[i * 8 + 12];
                    m_Ks[i * 8 + 14] = m_Ks[i * 8 + 5] ^ m_Ks[i * 8 + 6] ^ m_Ks[i * 8 + 12];
                    m_Ks[i * 8 + 15] = m_Ks[i * 8 + 7] ^ m_Ks[i * 8 + 14];
		        }
		        m_nCipherRnd = 14;
                indKs = 32 * 7;
	        }
	        else
	        {
		        m_nCipherRnd = 0;
		        return;
	        }
            m_Ke[0] = m_Ks[indKs / 4];
            m_Ke[1] = m_Ks[indKs / 4 + 1];
            m_Ke[2] = m_Ks[indKs / 4 + 2];
            m_Ke[3] = m_Ks[indKs / 4 + 3];
            int ind = 0;
	        for(int i = (m_nCipherRnd - 1) * 4; i > 0; i--, ind++)
	        {
		        indKs += (((i & 3) != 0) ? 1 : -7) * 4;
                m_Ke[ind + 4] = s_tabIT[3, s_tabSB[bKs(indKs + 15)]] ^ s_tabIT[2, s_tabSB[bKs(indKs + 14)]] ^
                    s_tabIT[1, s_tabSB[bKs(indKs + 13)]] ^ s_tabIT[0, s_tabSB[bKs(indKs + 12)]];
        	}
            m_Ke[ind + 4] = m_Ks[indKs / 4 - 4];
            m_Ke[ind + 5] = m_Ks[indKs / 4 - 3];
            m_Ke[ind + 6] = m_Ks[indKs / 4 - 2];
            m_Ke[ind + 7] = m_Ks[indKs / 4 - 1];
        }

        public byte[] EncryptData(byte[] data, byte[] key)
        {
	        EncodeKey(key, key.Length * 8);
            byte[] buf = new byte[(data.Length + 15) & ~15];
	        byte[] block = new byte[16];
            int ib = 0;
	        for(int i = 0; i < data.Length / 16; i++, ib += 16)
	        {
		        for(int k = 0; k < 16; k++)
			        block[k] ^= data[ib + k];
		        block = EncryptBlock(block);
                Array.Copy(block, 0, buf, ib, 16);
	        }
	        if((data.Length & 0xF) != 0)
	        {
		        for(int i = 0; i < (data.Length & 0xF); i++)
			        block[i] ^= data[ib + i];
		        block = EncryptBlock(block);
                Array.Copy(block, 0, buf, ib, 16);
	        }
	        return buf;
        }

		public byte[] DecryptData(byte[] data, byte[] key)
		{
			EncodeKey(key, key.Length * 8);
			byte[] buf = new byte[data.Length];
			byte[] block0 = new byte[16];
			byte[] block1 = new byte[16];
			byte[] block2 = new byte[16];
			byte[] block3 = new byte[16];
			byte[] res = new byte[16];
			int ib = 0;
			for (int i = data.Length / 16; i > 0; i--, ib += 16)
			{
				if ((i & 1) == 0)
				{
					Array.Copy(data, ib, block0, 0, 16);
					Array.Copy(data, ib, block2, 0, 16);
					block2 = DecryptBlock(block2);
					for (int k = 0; k < 16; k++)
						res[k] = (byte)(block1[k] ^ block2[k]);
				}
				else
				{
					Array.Copy(data, ib, block1, 0, 16);
					Array.Copy(data, ib, block3, 0, 16);
					block3 = DecryptBlock(block3);
					for (int k = 0; k < 16; k++)
						res[k] = (byte)(block0[k] ^ block3[k]);
				}
				Array.Copy(res, 0, buf, ib, 16);
			}
			return buf;
		}
	}

	internal class vCRC32
    {
        static uint[] CRCTable = new uint[]
        {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
            0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
            0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
            0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
            0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
            0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
            0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
            0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
            0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
            0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
            0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
            0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
            0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
            0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
            0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
            0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
            0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
            0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
            0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
            0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
            0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
            0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
            0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
            0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
            0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
            0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
        };
  
        public static uint Calculate(byte[] data, uint crc)
        {
            uint CRCVal = ~crc;
            for (int i = 0; i < data.Length; i++)
            {
                CRCVal = (CRCVal >> 8) ^ CRCTable[(CRCVal & 0xff) ^ data[i]];
            }
            return ~CRCVal;
        }
    }
}
