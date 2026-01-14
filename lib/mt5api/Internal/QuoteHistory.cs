using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    class QuoteHistory
    {
        readonly MT5API QuoteClient;

        internal QuoteHistory(MT5API qc)
        {
            QuoteClient = qc;
        }

        internal static async Task ReqTicks(Connection connection, uint id)
        {
            OutBuf buf = new OutBuf();
            buf.ByteToBuffer(9);
            if (id == 0)
                buf.LongToBuffer(0);
            else
            {
                buf.LongToBuffer(1);
                buf.LongToBuffer(id);
            }
            await connection.SendPacket(0x69, buf);
        }

        internal static async Task ReqProcess(Connection connection, string symbol)
        {
            OutBuf buf = new OutBuf();
            buf.Add((byte)0xE);
            var bytes = Encoding.Unicode.GetBytes(symbol);
            buf.Add(bytes);
            buf.Add(new byte[32 * 2 - bytes.Length]);
            buf.Add((ushort)1); //size
            buf.Add((byte)0); //year
            buf.Add((int)0); //time
            buf.Add((ushort)0); //CheckDate
            await connection.SendPacket(0x66, buf);
        }

        internal static async Task ReqStart(Connection connection, string symbol, ushort date)
        {
            OutBuf buf;
            buf = new OutBuf();
            buf.Add((byte)0xE);
            var bytes = Encoding.Unicode.GetBytes(symbol);
            buf.Add(bytes);
            buf.Add(new byte[32 * 2 - bytes.Length]);
            buf.Add((ushort)0); //size
           // buf.Add((byte)0); //year
            //buf.Add((int)0); //time
            buf.Add(date); //CheckDate
            await connection.SendPacket(0x66, buf);
        }

        internal static async Task ReqSend(Connection connection, string symbol, ushort a, ushort b)
        {
            OutBuf buf = new OutBuf();
            buf.Add((byte)9);
            var bytes = Encoding.Unicode.GetBytes(symbol);
            buf.Add(bytes);
            buf.Add(new byte[32 * 2 - bytes.Length]);
            buf.Add((ushort)a); //begin
            buf.Add((ushort)b); //firstDate
            await connection.SendPacket(0x66, buf);
        }

        internal void Parse(InBuf buf)
        {
            while (buf.hasData)
            {
                var cmd = buf.Byte();
                var symbol = ConvertBytes.ToUnicode(buf.Bytes(64));
                switch (cmd)
                {
                    case 0x0E:
                        QuoteClient.OnQuoteHistoryCall(symbol, ParseStart(buf, symbol));
                        return;
                    case 9:
                        QuoteClient.OnQuoteHistoryCall(symbol, ParseSelect(buf, symbol));
                        break;
                    default:
                        throw new NotImplementedException("Parse quote hist cmd = " + cmd);
                }
            }
        }

        internal List<Bar> ParseSelect(InBuf buf, string symbol)
        {
            var status = buf.Int();
            if (status != 0)
                throw new Exception(((Msg)status).ToString());
            var num = buf.UShort();
            //Console.WriteLine(num);
            //if (num == 0)
            //    throw new Exception("num == 0");
            List<Bar> bars = new List<Bar>();
            for (int i = 0; i < num; i++)
              bars.InsertRange(0, ParseContainer(buf, symbol));
            return bars;
        }

        internal List<Bar> ParseStart(InBuf buf, string symbol)
        {
            var status = buf.Int();
            if (status == 1)
                return new List<Bar>();
            if (status != 0)
                throw new Exception(((Msg)status).ToString());
            var day = buf.UShort();
            var num = buf.UShort();
            if (num == 0)
                return ParseContainer(buf, symbol);
            else
                throw new NotImplementedException("num != 0");
        }

        List<Bar> ParseContainer(InBuf buf, string symbol)
        {
            var hdr = UDT.ReadStruct<HistHeader>(buf.Bytes(0x81), 0, 0x81);
            if ((hdr.Flags & 1) != 0)
                throw new Exception("Compressed");
            byte[] data = buf.Bytes(hdr.DataSize);
            return ReadBarRecords(new BitReader(data, hdr), hdr, symbol);
        }

        List<Bar> ReadBarRecords(BitReader btr, HistHeader hdr, string symbol)
        {
            ulong flags = 0;
            bool bSpread = false;
            bool bVolume = false;
            int numBars = 0;
            List<Bar> bars = new List<Bar>();
            BarRecord rec = new BarRecord();
            while ((btr.BitPos <= btr.BitSize) && (numBars < hdr.NumberBars))
            {
                long type = BitConverter.ToInt64(btr.GetRecord(8), 0);
                if (type == 0)
                {
                    flags = BitConverter.ToUInt64(btr.GetRecord(8), 0);
                    rec.Time = BitConverter.ToInt64(btr.GetRecord(8), 0);
                    rec.OpenPrice = BitConverter.ToInt32(btr.GetSignRecord(8), 0);
                    rec.High = BitConverter.ToInt32(btr.GetRecord(4), 0);
                    rec.Low = BitConverter.ToInt32(btr.GetRecord(4), 0);
                    rec.Close = BitConverter.ToInt32(btr.GetSignRecord(4), 0);
                    rec.TickVolume = BitConverter.ToUInt64(btr.GetRecord(8), 0);
                    if ((flags & 1)!=0)
                    {
                        bSpread = true;
                        rec.Spread = BitConverter.ToInt32(btr.GetSignRecord(4), 0);
                    }
                    if ((flags & 2)!=0)
                    {
                        bVolume = true;
                        rec.Volume = BitConverter.ToUInt64(btr.GetRecord(8), 0);
                    }
                    btr.SkipRecords(flags, 4);
                    bars.Add(RecordToBar(rec, hdr.Digits));
                    numBars++;
                }
                else if (type == 1)
                {
                    long num = btr.GetLong();
                    for (long i = 0; i < num; i++)
                    {
                        rec.Time += 60;
                        long value = btr.GetSignLong();
                        rec.OpenPrice += hdr.LimitPoints * value + rec.Close;
                        int data = btr.GetInt();
                        rec.High = (int)(hdr.LimitPoints * data);
                        data = btr.GetInt();
                        rec.Low = (int)(hdr.LimitPoints * data);
                        value = btr.GetSignLong();
                        rec.Close = (int)(hdr.LimitPoints * (int)value);
                        rec.TickVolume = btr.GetULong();
                        if (bSpread)
                            rec.Spread = btr.GetSignInt();
                        if (bVolume)
                            rec.Volume = btr.GetULong();
                        btr.SkipRecords(flags, 4);
                        bars.Add(RecordToBar(rec, hdr.Digits));
                        numBars++;
                    }
                }
                else if (type == 2)
                {
                    long value = btr.GetLong();
                    rec.Time += value * 60;
                }
            }
            return bars;
            
            //m_Hdr.m_nBitSize = btr.m_nBitPos;
            //if (!numBars)
            //    return true;
            //int i = firstPos;
            //while ((TimeToDate(arrBar[i].m_lTime) != m_Hdr.m_Date) && (i < arrBar.GetSize()))
            //    i++;
            //int removed = 0;
            //if (i != firstPos)
            //{
            //    removed = i - firstPos + 1;
            //    arrBar.ShrinkTo(firstPos, i);
            //}
            //if (arrBar.GetSize())
            //{
            //    i = arrBar.GetSize() - 1;
            //    while ((TimeToDate(arrBar[i].m_lTime) != m_Hdr.m_Date) && (i > firstPos))
            //        i--;
            //    if (i != arrBar.GetSize() - 1)
            //    {
            //        removed = arrBar.GetSize() - i;
            //        arrBar.SetSize(i);
            //    }
            //}
        }

        private Bar RecordToBar(BarRecord rec, int digits)
        {
            Bar bar = new Bar
            {
                Time = ConvertTo.DateTime(rec.Time),
                OpenPrice = ConvertTo.LongLongToDouble(digits, rec.OpenPrice),
                HighPrice = ConvertTo.LongLongToDouble(digits, rec.OpenPrice + rec.High),
                LowPrice = ConvertTo.LongLongToDouble(digits, rec.OpenPrice - rec.Low),
                ClosePrice = ConvertTo.LongLongToDouble(digits, rec.OpenPrice + rec.Close),
                Volume = rec.Volume,
                TickVolume = rec.TickVolume,
                Spread = rec.Spread
            };
            return bar;
        }

		private TickBar TickRecordToBar(TickBarRecord rec, int digits)
		{
			TickBar bar = new TickBar
			{
				Time = ConvertTo.DateTimeMs(rec.TimeMs),
				Bid = ConvertTo.LongLongToDouble(digits, rec.Bid),
				Ask = ConvertTo.LongLongToDouble(digits, rec.Ask),
				Last = ConvertTo.LongLongToDouble(digits, rec.Last),
				Volume = rec.Volume,
			};
			return bar;
		}

		public List<TickBar> ReadTickBarRecords(BitReader btr, ContainerHeader hdr, string symbol)
		{
			ulong flags = 0;
			bool bSpread = false;
			bool bVolume = false;
			int numBars = 0;
			List<TickBar> bars = new List<TickBar>();
			TickBarRecord rec = new TickBarRecord();
			while ((btr.BitPos <= btr.BitSize) && (numBars < hdr.NumberTicks))
			{
				long type = BitConverter.ToInt64(btr.GetRecord(8), 0);
				if (type == 5)
				{
					rec.TimeMs = BitConverter.ToInt64(btr.GetRecord(8), 0);
					rec.Bid = BitConverter.ToInt64(btr.GetSignRecord(8), 0);
					rec.Ask = BitConverter.ToInt64(btr.GetSignRecord(8), 0);
					rec.Last = BitConverter.ToInt64(btr.GetSignRecord(8), 0);
					rec.Volume = BitConverter.ToUInt64(btr.GetRecord(8), 0);
					rec.UpdataMask = BitConverter.ToUInt64(btr.GetRecord(8), 0);
					rec.BankId = BitConverter.ToUInt32(btr.GetRecord(4), 0);
					rec.s38 = btr.GetSignRecord(1)[0];
					var value = BitConverter.ToUInt64(btr.GetRecord(8), 0);
					rec.Volume = rec.Volume * 100000000 + value;
					bars.Add(TickRecordToBar(rec, hdr.Digits));
					numBars++;
				}
				else
					throw new NotImplementedException();
			}
			return bars;
		}
	}
}
/*
 * bool vHistorySymbol::SendRequest()
{
	if (m_bAbsentSymbol || (m_pClient->GetServerStatus() != vAcceptAccount) ||
		(m_History.m_Mode == vMode_StartRequest) || m_History.m_BeginHistory.IsEmpty())
		return false;
	vDate firstDate = m_History.m_BeginDay;
	if (firstDate != vDate(0, 1, 1))
		firstDate = TimeToDate(DateToTime(firstDate) - 24 * 3600);
	vDate begin = vDate(0, 1, 1);
	if ((m_History.m_BeginHistory != vDate(0, 1, 1)) && (firstDate != vDate(0, 1, 1)))
	{
		begin = m_History.m_BeginHistory;
		if (firstDate < begin)
			return false;
	}
	vSockBufManager bufMan(0);
	bufMan.ByteToBuffer(9);
	bufMan.DataToBuffer(m_Symbol.m_SymInfo.m_sCurrency, 32 * sizeof(wchar_t));
	bufMan.DataToBuffer(&begin, 2);
	bufMan.DataToBuffer(&firstDate, 2);
#ifdef DEBUG_QH
	vString<256> sDate1, sDate2;
	s_pLogger->LogMessage(vMessage, L"vHistory", L"SendRequest: 9 %s, Date1 %s, Date2 %s", m_Symbol.m_SymInfo.m_sCurrency,
		sDate1.DateToString(DateToTime(begin)), sDate2.DateToString(DateToTime(firstDate)));
#endif
	if (m_pClient->SendPacket(0x66, &bufMan))
		return true;
	s_pLogger->LogMessage(vFatal, L"History", L"'%s' request sending failed [%u]", m_Symbol.m_SymInfo.m_sCurrency, GetLastError());
	return false;
}

 */
