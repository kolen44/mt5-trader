using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5.Internal
{
	//internal class TickHistory
	//{

	//	readonly MT5API QuoteClient;

	//	internal TickHistory(MT5API qc)
	//	{
	//		QuoteClient = qc;
	//	}

	//	public void RequestTickHistory(string symbol, int year, int month, int day, uint count)
	//	{
	//		OutBuf buf = new OutBuf();
	//		buf.ByteToBuffer(0xE);
	//		var bytes = Encoding.Unicode.GetBytes(symbol);
	//		buf.Add(bytes);
	//		buf.Add(new byte[32 * 2 - bytes.Length]);
	//		buf.Add(Date.Convert(year, month, day));
	//		buf.LongToBuffer(count);
	//		buf.LongToBuffer(0);
	//		QuoteClient.Connection.SendPacket(0x69, buf).Wait();
	//	}
	//}
}
