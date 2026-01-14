using mtapi.mt5.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	public partial class MT5API
	{
		internal ConcurrentDictionary<string, TickHistReq> TickHistRequests = new ConcurrentDictionary<string, TickHistReq>();

		public void TickHistoryRequest(string symbol, int startYear, int startMonth, int startDay)
		{
			if (!TickHistRequests.TryAdd(symbol, new TickHistReq(symbol, startYear, startMonth, startDay)))
				throw new Exception($"Previuos quote history request for {symbol} is still running");
			new TickHistory(this).RequestTickHistory(symbol, startYear, startMonth, startDay, 0);
		}

		public void TickHistoryStop(string symbol)
		{
			TickHistRequests.TryRemove(symbol, out _);
		}
	}

	internal class TickHistReq
	{
		public List<Container> Containers = new List<Container>();
		public string Symbol;
		public int Year;
		public int Month;
		public int Day;

		public TickHistReq( string symbol, int year, int month, int day)
		{
			Symbol = symbol;
			Year = year;
			Month = month;
			Day = day;
		}
	}
}
