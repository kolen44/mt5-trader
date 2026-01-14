using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace mtapi.mt5
{
    class InBuf
    {
        public byte[] Buf;
        protected int Ind;
        public PacketHdr Hdr;
        public short SymBuild;
        public virtual int CurrentIndex => Ind;

        public InBuf(byte[] buf, int start)
        {
            Buf = buf;
            Ind = start;
        }

        public T Struct<T>() where T : FromBufReader, new()
        {
            var res = UDT.ReadStruct<T>(this);
            return res;
        }

        public T CryptStruct<T>(int size) where T : FromBufReader, new()
        {
            var bytes = Bytes(size);
            Crypt.EasyDecrypt(bytes);
            return UDT.ReadStruct<T>(bytes, 0, size);
        }

        public T[] CryptArray<T>(int size) where T : FromBufReader, new()
        {
            int num = Int();
            T[] ar = new T[num];
            for (int i = 0; i < num; i++)
                ar[i] = CryptStruct<T>(size);
            return ar;
        }

        public T[] Array<T>() where T : FromBufReader, new()
        {
            int num = Int();
            T[] ar = new T[num];
            for (int i = 0; i < num; i++)
                ar[i] = Struct<T>();
            return ar;
        }

        public Dictionary<long, ConcurrentDictionary<long, DealInternal>> ArrayDeal()
        {
            int num = Int();
            var res = new Dictionary<long, ConcurrentDictionary<long, DealInternal>>();
            for (int i = 0; i < num; i++)
            {
                var deal = Struct<DealInternal>();
                if (!res.ContainsKey(deal.PositionTicket))
                    res.Add(deal.PositionTicket, new ConcurrentDictionary<long, DealInternal>());
                res[deal.PositionTicket].TryAdd(deal.TicketNumber, deal);
            }
            return res;
        }

        internal T CryptStruct<T>(object size)
        {
            throw new NotImplementedException();
        }

        public void SetBuf(byte[] buf)
        {
            Buf = buf;
            Hdr.PacketSize = buf.Length;
            Ind = 0;
        }

        public InBuf(byte[] buf, PacketHdr hdr)
        {
            Buf = buf;
            Hdr = hdr;
        }

        public byte[] ToBytes()
        {
            return Buf;
        }

        public virtual byte Byte()
        {
            if (Ind >= Buf.Length)
                throw new Exception("Not enough data");
            return Buf[Ind++];
        }

        public virtual int Left
        {
            get
            {
                return Buf.Length - Ind;
            }
        }

        internal string String(int v)
        {
            throw new NotImplementedException();
        }

        public byte[] Bytes(int count)
        {
            var res = new byte[count];
            for (int i = 0; i < count; i++)
                res[i] = Byte();
            return res;
        }

        public int Int()
        {
            int res = BitConverter.ToInt32(Bytes(4), 0);
            return res;
        }

        public long Long()
        {
            long res = BitConverter.ToInt64(Bytes(8), 0);
            return res;
        }

        public double Double()
        {
            var res = BitConverter.ToDouble(Bytes(8), 0);
            return res;
        }

        public ulong ULong()
        {
            ulong res = BitConverter.ToUInt64(Bytes(8), 0);
            return res;
        }

        public ushort UShort()
        {

            ushort res = BitConverter.ToUInt16(Bytes(2), 0);
            return res;

        }

        public string Str()
        {
            int sz = Int();
            var res = Encoding.Unicode.GetString(Bytes(sz));
            return res;
        }

        public byte[] ByteAr()
        {
            int sz = Int();
            var res = new byte[sz];
            for (int i = 0; i < sz; i++)
                res[i] = Byte();
            return res;

        }

        public virtual bool hasData
        {
            get
            {
                return Ind < Buf.Length;
            }
        }

        //internal void AddRange(List<byte> list)
        //{
        //    if (Ind > 0)
        //        throw new Exception("Cannot insert");
        //    list.AddRange(Buf);
        //    Buf = list.ToArray();
        //}

        //public void Skip()
        //{
        //    Ind += Int();
        //}
    }
}
