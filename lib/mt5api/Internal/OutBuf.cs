using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    public class OutBuf
    {
        internal List<byte> List;

        internal OutBuf()
        {
            List = new List<byte>();
        }

        internal OutBuf(byte[] bytes)
        {
            List = new List<byte>(bytes);
        }

        public void CreateHeader(byte type, int id, bool compressed)
        {
            byte[] hdr = new byte[9];
            hdr[0] = type; //type
            BitConverter.GetBytes(List.Count).CopyTo(hdr, 1); //size
            BitConverter.GetBytes((ushort)id).CopyTo(hdr, 5); //ID
            if(compressed)
                BitConverter.GetBytes((ushort)3).CopyTo(hdr, 7);
            else
                BitConverter.GetBytes((ushort)2).CopyTo(hdr, 7); //Flags PHF_COMPLETE
            List.InsertRange(0, hdr);
        }

        public byte[] ToArray()
        {
            return List.ToArray();
        }
        
        public void Add(byte b)
        {
            List.Add(b);
        }

        public void Add(ulong l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(long l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(int l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(short l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(ushort l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(uint l)
        {
            Add(BitConverter.GetBytes(l));
        }

        public void Add(byte[] bytes)
        {
            List.AddRange(bytes);
        }

        internal void ByteToBuffer(byte v)
        {
            Add(v);
        }

        internal void LongToBuffer(uint v)
        {
            Add(v);
        }

        internal void IntToBuffer(int v)
        {
            Add(v);
        }

        internal void WordToBuffer(ushort v)
        {
            Add(v);
        }

        internal void LongLongToBuffer(long v)
        {
            Add(v);
        }

        internal void DataToBuffer(byte[] v)
        {
            Add(v);
        }

		internal void Add(string str, int len)
		{
			var res = new byte[len*4];
            if (str != null)
            {
                var bytes = Encoding.Unicode.GetBytes(str);
                if (bytes.Length > res.Length)
                    Array.Resize(ref bytes, res.Length);
                bytes.CopyTo(res, 0);
            }
			Add(res);
		}

		internal void Add(double price)
		{
			Add(BitConverter.GetBytes(price));
		}

		internal void Add(byte[] ar, int len)
		{
			var res = new byte[len];
			if(ar!=null)
			ar.CopyTo(res, 0);
			Add(res);
		}
	}
}
