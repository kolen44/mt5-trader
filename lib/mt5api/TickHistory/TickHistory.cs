using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace mtapi.mt5.Internal
{
	internal class TickHistory
	{

		readonly MT5API QuoteClient;

		internal TickHistory(MT5API qc)
		{
			QuoteClient = qc;
		}

		public void RequestTickHistory(string symbol, int year, int month, int day, uint count, Container[] exist = null)
		{
			OutBuf buf = new OutBuf();
			buf.ByteToBuffer(0xE);
			var bytes = Encoding.Unicode.GetBytes(symbol);
			buf.Add(bytes);
			buf.Add(new byte[32 * 2 - bytes.Length]);
			buf.Add(Date.Convert(day, month, year - 1970));
			buf.LongToBuffer(count); //ticks
			if(exist == null)
				buf.LongToBuffer(0); //numdays
			else
			{
				buf.LongToBuffer((uint)exist.Length);
				foreach (var item in exist)
					buf.DataToBuffer(GetTicksBase(item));
			}
			OutBuf hdr = new OutBuf();
			hdr.WordToBuffer(499);
			hdr.Add(bytes);
			hdr.Add(new byte[32 * 2 - bytes.Length]);
			hdr.Add(Date.Convert(day, month, year - 1970));
			hdr.Add(Date.Convert(day, month, year - 1970));
			hdr.IntToBuffer(0);
			hdr.IntToBuffer(0);
			hdr.IntToBuffer(0);
			hdr.IntToBuffer(0);
			hdr.ByteToBuffer((byte)2);
			hdr.IntToBuffer(0);
			hdr.WordToBuffer(5);
			hdr.WordToBuffer(6);
			hdr.LongToBuffer(1);
			buf.Add(hdr.ToArray());
			buf.Add(new byte[499 - hdr.List.Count]);
			QuoteClient.Connection.SendPacket(0x69, buf).Wait();
		}

		byte[] GetTicksBase(Container cont)
		{
			OutBuf buf = new OutBuf();	
			buf.WordToBuffer(cont.Header.Date);
			buf.WordToBuffer(0);
			buf.IntToBuffer(cont.Header.Time);
			buf.IntToBuffer(cont.Header.DataSize);
			buf.IntToBuffer(0); //offset
			buf.WordToBuffer((ushort)cont.Header.Flags);
			buf.WordToBuffer(0);
			buf.LongToBuffer((uint)cont.Header.Crc32);
			return buf.ToArray();
		}

		//struct vTicksDayBase                            //sizeof 0x18 h
		//{
		//	vDate m_Date;                       //0
		//	vDate s2;
		//	int m_nTime;                    //4
		//	int m_nContSize;                //8
		//	int m_nContOffset;              //C
		//	WORD m_nFlags;                  //10
		//	vDate s12;
		//	ULONG m_nCrc32;                 //14

		//	vTicksDayBase() { Clear(); }
		//	void Clear() { memset(this, 0, sizeof(vTicksDayBase)); }
		//	static int FindByDate(const void* pKey, const void* pObj);
		//	static int CompareByDate(const void* t1, const void* t2);
		//};

	}
}
