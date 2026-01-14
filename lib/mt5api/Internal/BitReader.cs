using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    class BitReader
    {
        byte Align;
        byte Blank;
        byte[] Data;
        internal int BitSize;

        internal int BitPos = 0;

		public BitReader(byte[] data, byte alignBit, int bitSize)
		{
			Data = data;
			Align = alignBit;
			Blank = (byte)((1 << Align) - 1);
			BitSize = bitSize;
		}

		public BitReader(byte[] data, HistHeader hdr)
        {
            Data = data;
            Align = hdr.AlignBit;
            Blank = (byte)((1 << hdr.AlignBit) - 1);
            BitSize = hdr.BitSize;
        }


        public BitReader(byte[] data, int bitSize)
        {
            Data = data;
            BitSize = bitSize;
        }

        internal long GetLong()
        {
            return BitConverter.ToInt64(GetRecord(8), 0);
        }

        internal ulong GetULong()
        {
            return BitConverter.ToUInt64(GetRecord(8), 0);
        }

        internal int GetInt()
        {
            return BitConverter.ToInt32(GetRecord(4), 0);
        }

        internal short GetShort()
        {
            return BitConverter.ToInt16(GetRecord(2), 0);
        }

		internal short GetByte()
		{
			return GetRecord(1)[0];
		}

		internal int GetSignInt()
        {
            return BitConverter.ToInt32(GetSignRecord(4), 0);
        }

        public long GetSignLong()
        {
            return BitConverter.ToInt64(GetSignRecord(8), 0);
        }

        public byte[] GetRecord(int size)
        {
            byte buf = 0;
            byte bitSize = 0;
            do
            {
                buf = ReadValue(Data, Align)[0];
                bitSize += buf;
            } while (buf == Blank);
            bitSize *= 2;
            if (size * 8 < bitSize)
                throw new Exception("size * 8 < bitSize");
            var res = ReadValue(Data, bitSize);
            return Ret(res, size);
        }

        internal byte[] ReadValue(byte[] data, int size)
        {
            if(size == 0)
                return new byte[0];
            if (size + BitPos > BitSize)
                throw new Exception("End of stream");
            int startBit = BitPos % 8;
            int bits = 0;
            int valueInd = 0;
            byte[] value = new byte[size];
            int dataInd = BitPos / 8;
            while (size > 0)
            {
                for (int i = startBit; i < 8; i++)
                {
                    if (size == 0)
                        break;
                    if (bits >= 8)
                    {
                        valueInd++;
                        bits = 0;
                    }
                    var c = data[dataInd];
                    byte ch = data[dataInd];
                    if ((ch & (1 << i)) > 0)
                        value[valueInd] |= (byte)(1 << bits);
                    else
                        value[valueInd] &= (byte)~(1 << bits);
                    BitPos++;
                    size--;
                    bits++;
                }
                if (size == 0)
                    break;
                dataInd++;
                startBit = 0;
            }
            return value;
        }

        internal void Initialize(byte align, byte blank)
        {
            Align = align;
            Blank = blank;
        }

        internal byte[] GetSignRecord(int size)
        {
            byte buf = 0;
            byte sign = 0;
            byte bitSize = 0;
            do
            {
                buf = ReadValue(Data, Align)[0];
                bitSize += buf;
            } while (buf == Blank);
            bitSize *= 2;
            if (size * 8 < bitSize)
                throw new Exception("size * 8 < bitSize");
            sign = ReadValue(Data, 1)[0];
            byte[] data = Ret(ReadValue(Data, bitSize), size);
            if (sign == 0)
                return data;
            switch (size)
            {
                case 1:
                    data[0] = (byte)-data[0];
                    break;
                case 2:
                    short vs = BitConverter.ToInt16(data, 0);
                    BitConverter.GetBytes(-vs).CopyTo(data, 0);
                    break;
                case 4:
                    int vi = BitConverter.ToInt32(data, 0);
                    BitConverter.GetBytes(-vi).CopyTo(data, 0);
                    break;
                case 8:
                    long vl = BitConverter.ToInt64(data, 0);
                    BitConverter.GetBytes(-vl).CopyTo(data, 0);
                    break;
            }
            return data;
        }

        internal void AlignBitPosition(int pos)
        {
            BitPos = (BitPos / 8 + pos) * 8;
        }

        private byte[] Ret(byte[] data, int size)
        {
            if (data.Length == 0)
                return new byte[size];
            var result = new byte[size];
            if (data.Length < result.Length)
                data.CopyTo(result, 0);
            else
                Array.Copy(data, 0, result, 0, result.Length);
            return result;
        }

        internal void SkipRecords(ulong mask, ulong startFlag)
        {
            startFlag <<= 1;
            if ((mask < startFlag))
                return;
            while (startFlag != 0)
            {
                if (startFlag > 0x8000000000000000)
                    break;
                if ((startFlag & mask) != 0)
                {
                    ulong value = BitConverter.ToUInt64(GetRecord(8), 0);

                }
                startFlag <<= 1;
            }
        }
    }
}
