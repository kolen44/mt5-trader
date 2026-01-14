using Ionic.Zlib;
using mtapi.mt5.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace mtapi.mt5
{

    internal class Container
    {
        public ContainerHeader Header;
        public HourHeader[] HourHeaders; 
        public ZipInfo ZipInfo;
        public List<ZipRecord> ZipRecords;
    }

    public class TickBarRecord                    //sizeof 0x39 h
    {
        public long TimeMs;         //0
        public int Index;           //8
        public long Bid;                //C
        public long Ask;                //14
        public long Last;           //1C
        public ulong Volume;            //24
        public ulong UpdateMask;        //2C
        public uint BankId;         //34
        public byte s38;
        internal ulong UpdataMask;
    }

    public class TickBar
    {
		public DateTime Time;
		public double Bid;
		public double Ask;
		public double Last;
		public ulong Volume;
		internal ulong UpdateMask;
	}

    internal class TickParser
    {
		MT5API Api;

        public TickParser(MT5API api) 
        {
            Api = api;
        }

        public void Parse(InBuf buf)
        {
            var status = buf.Int();
            var cmd = buf.Byte();
            if (cmd == 0xE)
            {
                var symbol = FromBufReader.GetString(buf.Bytes(64));
                var b = buf.Long();
                var begin = ConvertTo.DateTimeMs(b);
                var date = buf.UShort(); //Date.Convert(buf.UShort());
                var reqdate = buf.UShort(); //Date.Convert(buf.UShort());
                var ticks = buf.Int();
                var num = buf.Int();
                if (num == 0)
                    ToString();
                for (int i = 0; i < num; i++)
                {
                    var cont = ParseContainer(buf);
                    if(Api.TickHistRequests.TryGetValue(symbol, out var req))
                        req.Containers.Add(cont);
					UnpackTickBars(cont);
				}
                if (status == 14)
                    if(Api.TickHistRequests.TryGetValue(symbol, out var req))
                        new TickHistory(Api).RequestTickHistory(req.Symbol, req.Year, req.Month, req.Day, 0, req.Containers.ToArray());
                try
                {
                    if (!buf.hasData)
                        return;
					var h = UDT.ReadStruct<ContainerHeader>(buf);
					if (h.Flags.HasFlag(HeaderFlags.ZIP_BY_HOURS))
					{
						var hourHeaders = new HourHeader[24];
						for (int i = 0; i < 24; i++)
							hourHeaders[i] = UDT.ReadStruct<HourHeader>(buf);
						for (int i = 0; i < 24; i++)
						{
							if (hourHeaders[i].ObjNumber > 0)
							{
								var zipinfo = UDT.ReadStruct<ZipInfo>(buf);
								var zipRecords = new List<ZipRecord>();
								for (int j = 0; j < zipinfo.NumRecords; j++)
									zipRecords.Add(buf.Struct<ZipRecord>());
								uint total = 0;
								foreach (var item in zipRecords)
								{
									total += item.PackSize;
									if (total > zipinfo.DataSize)
										throw new Exception("Wrong zip records");
									var bytes = buf.Bytes((int)item.PackSize);
									using (var compressedStream = new MemoryStream(bytes))
									using (var zipStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress))
									using (var resultStream = new MemoryStream())
									{
										zipStream.CopyTo(resultStream);
										item.Data = resultStream.ToArray();
									}
								}
								var cont = new Container() { Header = h, ZipInfo = zipinfo, ZipRecords = zipRecords };
								UnpackTickBars(cont);
							}
						}
					}
					if (!buf.hasData)
						return;
					ParseZeroFlag(buf);
				}
                finally
                { 
					if (status != 14)
					{
						Api.OnTickHistoryCall(symbol, new TickBar[0]);
						Api.TickHistRequests.TryRemove(symbol, out _);
					}
				}
			}
        }

        void ParseZeroFlag(InBuf buf)
        {
            var hdr = UDT.ReadStruct<ContainerHeader>(buf);
            if (hdr.Flags.HasFlag(HeaderFlags.ZIP_PACKED) || hdr.Flags.HasFlag(HeaderFlags.ZIP_BY_HOURS))
                return;
            var bytes = buf.Bytes(hdr.DataSize);
            var br = new BitReader(bytes, hdr.AlignBit, hdr.BitSize);
            var bars = new QuoteHistory(Api).ReadTickBarRecords(br, hdr, hdr.Currency);
			Api.OnTickHistoryCall(hdr.Currency, bars.ToArray());
        }

        Container ParseContainer(InBuf buf)
        {
            var hdr = UDT.ReadStruct<ContainerHeader>(buf);
            if (hdr.Flags.HasFlag(HeaderFlags.ZIP_PACKED) && !hdr.Flags.HasFlag(HeaderFlags.ZIP_BY_HOURS))
            {
                var zipinfo = UDT.ReadStruct<ZipInfo>(buf);
                var zipRecords = new List<ZipRecord>();
                for (int i = 0; i < zipinfo.NumRecords; i++)
                    zipRecords.Add(buf.Struct<ZipRecord>());
                uint total = 0;
                foreach (var item in zipRecords)
                {
                    total += item.PackSize;
                    if (total > zipinfo.DataSize)
                        throw new Exception("Wrong zip records");
					var bytes = buf.Bytes((int)item.PackSize);
                    using (var compressedStream = new MemoryStream(bytes))
                    using (var zipStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        item.Data = resultStream.ToArray();
                    }
                }
				var cont = new Container() { Header = hdr, ZipInfo = zipinfo, ZipRecords = zipRecords};
				return cont;
			}
            else
                throw new Exception("Non zip packed");
        }

        public void UnpackTickBars(Container cont, int flags = 479)
        {
            string symbol = cont.Header.Currency;
			var objNum = cont.ZipInfo.ObjNumber;
            if (objNum > 0)
            {
                var barArr = new TickBar[objNum];
                for (int i = 0; i < objNum; i++)
                {
                    var bar = new TickBar();
                    barArr[i] = bar;
                }
                var timeRec = GetZipRecord(1, cont.ZipRecords);
                if (timeRec != null)
                    UnpackTime(timeRec, barArr, objNum);
				var volRec = GetZipRecord(0x10, cont.ZipRecords);
				if (volRec != null)
					UnpackVolume(volRec, barArr, objNum);
				var maskRec = GetZipRecord(0x40, cont.ZipRecords);
				if (maskRec != null)
					UnpackUpdateMask(maskRec, barArr, objNum);
				var bidRec = GetZipRecord(2, cont.ZipRecords);
				if (bidRec != null)
					UnpackBid(bidRec, barArr, objNum);
				var askRec = GetZipRecord(4, cont.ZipRecords);
				if (askRec != null)
					UnpackAsk(askRec, barArr, objNum); 
                var lastRec = GetZipRecord(8, cont.ZipRecords);
				if (lastRec != null)
					UnpackLast(lastRec, barArr, objNum);
                Api.OnTickHistoryCall(symbol, barArr);
			}
        }

        ZipRecord GetZipRecord(int type, List<ZipRecord> list) 
        {
            foreach (var item in list)
                if(item.Type == type)
                    return item;
            return null;
        }

        void UnpackTime(ZipRecord rec, TickBar[] ticks, uint numRec)
        {
            var pData = new InBuf(rec.Data, 0);
            long prevTime = 0;
            for (uint i = 0; i < numRec; i++)
            {
				var time = pData.Long();
                time += prevTime;
				ticks[i].Time = ConvertTo.DateTimeMs(time);
                prevTime = time;
            }
        }

		void UnpackVolume(ZipRecord rec, TickBar[] ticks, uint numRec)
		{
			var pData = new InBuf(rec.Data, 0);
			for (uint i = 0; i < numRec; i++)
				ticks[i].Volume = pData.ULong();
		}

		void UnpackUpdateMask(ZipRecord rec, TickBar[] ticks, uint numRec)
		{
			var pData = new InBuf(rec.Data, 0);
			for (uint i = 0; i < numRec; i++)
				ticks[i].UpdateMask = pData.ULong();
		}

		void UnpackBid(ZipRecord rec, TickBar[] ticks, uint numRec)
		{
			var pData = new InBuf(rec.Data, 0);
			for (uint i = 0; i < numRec; i++)
				ticks[i].Bid = pData.Double();
		}

		void UnpackAsk(ZipRecord rec, TickBar[] ticks, uint numRec)
		{
			var pData = new InBuf(rec.Data, 0);
			for (uint i = 0; i < numRec; i++)
				ticks[i].Ask = pData.Double();
		}

		void UnpackLast(ZipRecord rec, TickBar[] ticks, uint numRec)
		{
			var pData = new InBuf(rec.Data, 0);
			for (uint i = 0; i < numRec; i++)
				ticks[i].Last = pData.Double();
		}

    }
}
