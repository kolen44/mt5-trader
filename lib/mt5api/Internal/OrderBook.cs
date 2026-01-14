using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace mtapi.mt5
{
	internal class OrderBook
	{

		readonly MT5API Api;

		internal OrderBook(MT5API api)
		{
			Api = api;
		}

		ConcurrentDictionary<string, SymbolBook> Books = new ConcurrentDictionary<string, SymbolBook>();
		ConcurrentDictionary<string, byte> Subscribed = new ConcurrentDictionary<string, byte>();

		public void Subscribe(string symbol)
		{
			Subscribed.TryAdd(symbol, 0);
			var keys = Subscribed.Keys;
			OutBuf buf = new OutBuf();
			buf.LongToBuffer((uint)keys.Count);
			foreach (var item in keys)
			{
				var id = Api.Symbols.GetInfo(item).Id;
				buf.LongToBuffer((uint)id);
			}
			Api.Connection.SendPacket(0x6A, buf).Wait();
		}

		public void Unsubscribe(string symbol)
		{
			if (Subscribed.TryRemove(symbol, out _))
			{
				var keys = Subscribed.Keys;
				OutBuf buf = new OutBuf();
				buf.LongToBuffer((uint)keys.Count);
				foreach (var item in keys)
				{
					var id = Api.Symbols.GetInfo(item).Id;
					buf.LongToBuffer((uint)id);
				}
				Api.Connection.SendPacket(0x6A, buf).Wait();
			}
		}

		public void ParseBooks(InBuf buf)
		{
			int rcvSize = buf.Left;
			if (rcvSize > 0)
			{
				byte[] data = buf.Bytes(buf.Left);
				var br = new BitReader(data, 2, data.Length * 8);
				while (br.BitPos < data.Length)
				{
					SymbolBook rec = ReadBook(br);
					if ((rec.Operation & 2)  > 0)
						continue;
					if (Books.TryGetValue(rec.Symbol, out var symbolBook))
					{
					}
					else
					{
						symbolBook = new SymbolBook() { Symbol = rec.Symbol };
						Books.TryAdd(rec.Symbol, symbolBook);
					}
					UpdateSymbolBook(symbolBook, rec.Bars);
					Api.OnOrderBookCall(symbolBook);
				}
			}
		}

		void UpdateSymbolBook(SymbolBook symbolBook, ConcurrentDictionary<double, BookBar> newBars)
		{
			foreach (var newBar in newBars.Values)
			{
				if (newBar.Volume == 0)
				{
					symbolBook.Bars.TryRemove(newBar.Price, out _);
					continue;
				}
				if(newBar.Type == BookBarType.Reset)
				{
					symbolBook.Bars.Clear();
					continue;
				}
				var bookBar = GetBarByPriceAndType(symbolBook, newBar.Price, newBar.Type);
				if (bookBar == null)
					symbolBook.Bars[newBar.Price] = newBar;	
				else
				{
					bookBar.Volume += newBar.Volume;
					if (bookBar.Volume == 0)
						symbolBook.Bars.TryRemove(newBar.Price, out _);	
				}
			}
		}

		BookBar GetBarByPriceAndType(SymbolBook rec, double price, BookBarType type)
		{
			if(rec.Bars.TryGetValue(price, out var bar))
				if (bar.Type == type)
					return bar;
			return null;
		}

		private SymbolBook ReadBook(BitReader br)
		{
			SymbolBook rec = new SymbolBook();
			rec.Symbol = Api.Symbols.GetInfo((int)br.GetInt()).Currency;
			rec.Time = br.GetLong();
			rec.UpdateMask = br.GetULong();
			var numBars = br.GetULong();
			for (ulong i = 0; i < numBars; i++)
			{
				BookBar bar = new BookBar();
				bar.UpdateMask = br.GetULong();
				bar.Type = (BookBarType)br.GetByte();
				bar.Price = Math.Round((double)(br.GetLong() * Api.Symbols.GetInfo(rec.Symbol).Points), 8);
				bar.Volume = (ulong)br.GetSignLong();
				bar.s19 = br.GetLong();
				rec.Bars[bar.Price] = bar;
			}
			return rec;
		}
	}

	/// <summary>
	/// Book record
	/// </summary>
	public class SymbolBook                             //sizeof 0x3C a
	{
		/// <summary>
		/// Id
		/// </summary>
		public string Symbol;          //0
		/// <summary>
		/// Time
		/// </summary>
		internal long Time;           //4
		//public long TimeMs;         //C
		/// <summary>
		/// Update mask
		/// </summary>
		internal ulong UpdateMask;        //14
		/// <summary>
		/// Bars
		/// </summary>
		public ConcurrentDictionary<double, BookBar> Bars = new ConcurrentDictionary<double, BookBar>();         //1C
		/// <summary>
		/// Operation
		/// </summary>
		internal long Operation;      //34
	}

	/// <summary>
	/// Book bar
	/// </summary>
	public class BookBar             //sizeof 0x21 a
	{
		/// <summary>
		///	Update mask
		/// </summary>
		public ulong UpdateMask;        //0
		/// <summary>
		/// Type
		/// </summary>
		public BookBarType Type;                //8
		/// <summary>
		/// Price
		/// </summary>
		public double Price;          //9
		/// <summary>
		/// Volume
		/// </summary>
		public ulong Volume;            //11
		/// <summary>
		/// Lot size
		/// </summary>
		public double Lots
		{
			get { return (double)Volume / 100000; }
		}
		internal long s19;
	}

	public enum BookBarType
	{
		Reset = 0,
		SellBook = 1,
		BuyBook = 2,
		SellMarket = 3,
		BuyMarket = 4
	}

	/// <summary>
	/// Tick history event args.
	/// </summary>
	public struct MarketDepthEventArgs
	{
		/// <summary>
		/// Instrument.
		/// </summary>
		public string Symbol;
		/// <summary>
		/// History bars.
		/// </summary>
		public SymbolBook BookRecord;
	}
}
