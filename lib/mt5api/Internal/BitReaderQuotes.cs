using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    class BitReaderQuotes
    {
        byte Align;
        byte Blank;
        byte[] Data;
        internal int BitSize;

        internal int BitPos = 0;


        public BitReaderQuotes(byte[] data, HistHeader hdr)
        {
            Data = data;
            Align = hdr.AlignBit;
            Blank = (byte)((1 << hdr.AlignBit) - 1);
            BitSize = hdr.BitSize;
        }


        public BitReaderQuotes(byte[] data, int bitSize)
        {
            Data = data;
            BitSize = bitSize;
        }

        internal bool GetLong(out long res)
        {
            byte[] buf;
            if (!GetRecord(8, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToInt64(buf, 0);
                return true;
            }
        }

        internal bool GetULong(out ulong res)
        {
            byte[] buf;
            if (!GetRecord(8, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToUInt64(buf, 0);
                return true;
            }
        }

        internal bool GetInt(out int res)
        {
            byte[] buf;
            if (!GetRecord(4, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToInt32(buf, 0);
                return true;
            }
        }

        internal bool GetShort(out short res)
        {
            byte[] buf;
            if (!GetRecord(2, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToInt16(buf, 0);
                return true;
            }
        }

        internal bool GetSignInt(out int res)
        {
            byte[] buf;
            if (!GetSignRecord(4, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToInt32(buf, 0);
                return true;
            }
        }

        internal bool GetSignLong(out long res)
        {
            byte[] buf;
            if (!GetSignRecord(8, out buf))
            {
                res = 0;
                return false;
            }
            else
            {
                res = BitConverter.ToInt64(buf, 0);
                return true;
            }
        }

        internal bool GetRecord(int size, out byte[] res)
        {
            byte buf = 0;
            byte bitSize = 0;
            do
            {
                byte[] value;
                if (!ReadValue(Data, Align, out value))
                {
                    res = null;
                    return false;
                }
                buf = value[0];
                bitSize += buf;
            } while (buf == Blank);
            bitSize *= 2;
            //if (size * 8 < bitSize)
            //    throw new Exception("size * 8 < bitSize");
            if (!ReadValue(Data, bitSize, out res))
            {
                res = null;
                return false;
            }
            res = Ret(res, size);
            return true; ;
        }

        internal bool ReadValue(byte[] data, int size, out byte[] res)
        {
            if (size + BitPos > BitSize)
            {
                res = null;
                return false;//throw new Exception("End of stream");
            }
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
            res = value;
            return true;
        }

        internal void Initialize(byte align, byte blank)
        {
            Align = align;
            Blank = blank;
        }

        internal bool GetSignRecord(int size, out byte[] res)
        {
            byte buf = 0;
            byte sign = 0;
            byte bitSize = 0;
            do
            {
                byte[] value;
                if (!ReadValue(Data, Align, out value))
                {
                    res = null;
                    return false;
                }
                buf = value[0];
                bitSize += buf;
            } while (buf == Blank);
            bitSize *= 2;
            if (size * 8 < bitSize)
                throw new Exception("size * 8 < bitSize");
            byte[] val;
            if (!ReadValue(Data, 1, out val))
            {
                res = null;
                return false;
            }
            sign = val[0];
            if (!ReadValue(Data, bitSize, out val))
            {
                res = null;
                return false;
            }
            byte[] data = Ret(val, size);
            if (sign == 0)
            {
                res = data;
                return true;
            }
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
            res = data;
            return true;
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
                    byte[] buf;
                    GetRecord(8, out buf);
                }
                startFlag <<= 1;
            }
        }
    }
}
