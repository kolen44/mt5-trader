//#define DELAYED_SYMBOLS
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class Subscriber
    {
        private readonly MT5API QuoteClient;
        internal readonly ConcurrentDictionary<string, Quote> Quotes = new ConcurrentDictionary<string, Quote>();
        private readonly Logger Log;
        internal readonly ConcurrentDictionary<string, Quote> LastQuotes = new ConcurrentDictionary<string, Quote>();
        internal readonly ConcurrentDictionary<string, MarketWatch> MarketWatches = new ConcurrentDictionary<string, MarketWatch>();
        static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Quote>> LastQuotesByServerName = new ConcurrentDictionary<string, ConcurrentDictionary<string, Quote>>();
        static readonly ConcurrentDictionary<string, Quote> LastQuoteAnyServer = new ConcurrentDictionary<string, Quote>(StringComparer.InvariantCultureIgnoreCase);

        static internal Quote GetQuoteFromAnyServer(string symbol)
        {
            if(LastQuoteAnyServer.TryGetValue(symbol, out var quote))
                return quote;
            else
            {
                if(symbol.Length > 6)
                    symbol = symbol.Substring(0, 6);
                symbol = symbol.ToLower();
                foreach (var item in LastQuoteAnyServer.Values)
                {
                    var sym = item.Symbol;
                    if (sym.Length > 6)
                        sym = sym.Substring(0, 6);
                    sym = sym.ToLower();
                    if (symbol == sym)
                        return quote;
                }
            }
            return null;    
        }

        SemaphoreSlim RequestLock = new SemaphoreSlim(1, 1);
        //DateTime LastRequest = DateTime.MinValue;


        public Subscriber(MT5API quoteClient, Logger log)
        {
            Log = log;
            QuoteClient = quoteClient;
        }

        public async Task <MarketWatch> GetMarketWatch(string symbol)
        {
            if (MarketWatches.TryGetValue(symbol, out var marketWatch))
            {
                var quote = await GetQuoteInternal(symbol);
                if( quote != null)
                {
                    if(quote.Bid != 0)
                        marketWatch.Bid = quote.Bid;
                    if (quote.Ask != 0)
                        marketWatch.Ask = quote.Ask;
                }
                return marketWatch;
            }
            return null; 
        }

        public async Task<Quote> GetQuote(string symbol, int msNotOlder)
        {
            if (Quotes.TryAdd(symbol, null)) 
                await Request(Quotes);
            if (LastQuotes.TryGetValue(symbol, out var quote))
                if (quote != null)
                    if (msNotOlder == 0)
                        return RoundBidAsk(quote);
                    else if (DateTime.Now.Subtract(quote.CreationTime).TotalMilliseconds <= msNotOlder)
                        return RoundBidAsk(quote);
            string serverName = QuoteClient.ClusterSummary?.ServerName;
            if (serverName != null)
                if (LastQuotesByServerName.TryGetValue(serverName, out var quotes))
                    if (quotes.TryGetValue(symbol, out quote))
                        if (quote != null)
                            if (msNotOlder == 0)
                                return RoundBidAsk(quote);
                            else if (DateTime.Now.Subtract(quote.CreationTime).TotalMilliseconds <= msNotOlder)
                                return RoundBidAsk(quote);
            return null;
        }

        internal async Task<Quote> GetQuoteInternal(string symbol)
        {
            if (Quotes.TryAdd(symbol, null))
                await Request(Quotes);
            if (LastQuotes.TryGetValue(symbol, out var quote))
                return RoundBidAsk(quote);
            return null;
        }

        public bool Subscribed(string symbol)
        {
            return Quotes.ContainsKey(symbol);
        }

        public string[] Subscriptions()
        {
            return Quotes.Keys.ToArray();
        }

        public async Task<bool> Subscribe(string symbol)
        {
            if (Quotes.TryAdd(symbol, null)) // || DateTime.Now.Subtract(LastRequest).TotalSeconds >= 5
            {
                await Request(Quotes);
                return true;
            }
            else
                return false;
        }

        public async Task Subscribe(string[] symbols)
        {
            foreach (var item in symbols)
                if(await Subscribe(item))
                    await Task.Delay(10);
        }

        internal async Task SubscribeForce(string[] symbols)
        {
            foreach (string symbol in symbols)
                if(QuoteClient.Symbols.Exist(symbol))
                    Quotes.TryAdd(symbol, null);
            await Request(Quotes);
        }


        public async Task Unsubscribe(string symbol)
        {
            bool found = false;
            foreach (var order in QuoteClient.GetOpenedOrders())
            {
                if ((order.OrderType == OrderType.Buy || order.OrderType == OrderType.Sell) && order.Symbol == symbol)
                    found = true;
            }
            if (!found)
                if (Quotes.TryRemove(symbol, out var tmp))
                    await Request(Quotes);
        }

        public async Task Unsubscribe(string[] symbols)
        {
            bool needToRequest = false;
            foreach (var symbol in symbols)
            {
                bool found = false;
                foreach (var order in QuoteClient.GetOpenedOrders())
                {
                    if ((order.OrderType == OrderType.Buy || order.OrderType == OrderType.Sell) && order.Symbol == symbol)
                        found = true;
                }
                if (!found)
                    if (Quotes.TryRemove(symbol, out var tmp))
                        needToRequest = true;
            }
            if (needToRequest)
                await Request(Quotes); ;
        }


        

        private async Task Request(ConcurrentDictionary<string, Quote> quotes) 
        {
            try
            {
                if (!await RequestLock.WaitAsync(5000))
                    throw new Exception("Subscriber.Request timeout");
                //LastRequest = DateTime.Now;
                OutBuf buf = new OutBuf();
                buf.ByteToBuffer(9);
                buf.IntToBuffer(quotes.Keys.Count);
                foreach (var item in quotes.Keys)
                {
                    var si = QuoteClient.Symbols.GetInfo(item);
                    buf.IntToBuffer(si.Id);
                }
                await QuoteClient.Connection.SendPacket(0x69, buf);
            }
            finally 
            {
                RequestLock.Release();
            }
            
        }

        public void ParseSymbolData(InBuf buf)
        {
            var bytes = buf.ToBytes();
            BitReaderQuotes br = new BitReaderQuotes(bytes, bytes.Length * 8);
            br.Initialize(2, 3);
            while (true)
            {
                var rec = new SymbolSummaryInternal();
                if (!br.GetInt(out rec.Id))
                    break;
                if (!br.GetULong(out rec.UpdateMask))
                    break;
                if (!br.GetLong(out rec.Time))
                    break;
                if ((rec.UpdateMask & 1) != 0)
                    if (!br.GetLong(out rec.Bid))
                        break;
                if ((rec.UpdateMask & 2) != 0)
                    if (!br.GetLong(out rec.BidHigh))
                        break;
                if ((rec.UpdateMask & 4) != 0)
                    if (!br.GetLong(out rec.BidLow))
                        break;
                if ((rec.UpdateMask & 8) != 0)
                    if (!br.GetLong(out rec.Ask))
                        break;
                if ((rec.UpdateMask & 0x10) != 0)
                    if (!br.GetLong(out rec.AskHigh))
                        break;
                if ((rec.UpdateMask & 0x20) != 0)
                    if (!br.GetLong(out rec.AskLow))
                        break;
                if ((rec.UpdateMask & 0x40) != 0)
                    if (!br.GetLong(out rec.Last))
                        break;
                if ((rec.UpdateMask & 0x80) != 0)
                    if (!br.GetLong(out rec.LastHigh))
                        break;
                if ((rec.UpdateMask & 0x100) != 0)
                    if (!br.GetLong(out rec.LastLow))
                        break;
                if ((rec.UpdateMask & 0x200) != 0)
                    if (!br.GetULong(out rec.Volume))
                        break;
                if ((rec.UpdateMask & 0x400) != 0)
                    if (!br.GetULong(out rec.VolumeHigh))
                        break;
                if ((rec.UpdateMask & 0x800) != 0)
                    if (!br.GetULong(out rec.VolumeLow))
                        break;
                if ((rec.UpdateMask & 0x1000) != 0)
                    if (!br.GetULong(out rec.BuyVolume))
                        break;
                if ((rec.UpdateMask & 0x2000) != 0)
                    if (!br.GetULong(out rec.SellVolume))
                        break;
                if ((rec.UpdateMask & 0x4000) != 0)
                    if (!br.GetULong(out rec.BuyOrders))
                        break;
                if ((rec.UpdateMask & 0x8000) != 0)
                    if (!br.GetULong(out rec.SellOrders))
                        break;
                if ((rec.UpdateMask & 0x10000) != 0)
                    if (!br.GetLong(out rec.Deals))
                        break;
                if ((rec.UpdateMask & 0x20000) != 0)
                    if (!br.GetULong(out rec.DealsVolume))
                        break;
                if ((rec.UpdateMask & 0x40000) != 0)
                    if (!br.GetLong(out rec.Turnover))
                        break;
                if ((rec.UpdateMask & 0x80000) != 0)
                    if (!br.GetLong(out rec.OpenInterest))
                        break;
                if ((rec.UpdateMask & 0x100000) != 0)
                    if (!br.GetLong(out rec.OpenPrice))
                        break;
                if ((rec.UpdateMask & 0x200000) != 0)
                    if (!br.GetLong(out rec.ClosePrice))
                        break;
                if ((rec.UpdateMask & 0x400000) != 0)
                    if (!br.GetLong(out rec.AverageWeightPrice))
                        break;
                if ((rec.UpdateMask & 0x800000) != 0)
                    if (!br.GetLong(out rec.PriceChange))
                        break;
                if ((rec.UpdateMask & 0x1000000) != 0)
                    if (!br.GetLong(out rec.PriceVolatility))
                        break;
                if ((rec.UpdateMask & 0x2000000) != 0)
                    if (!br.GetLong(out rec.PriceTheoretical))
                        break;
                if ((rec.UpdateMask & 0x8000000) != 0)
                    if (!br.GetLong(out rec.PriceChange))
                        break;
                if ((rec.UpdateMask & 0x4000000) != 0)
                    if (!br.GetLong(out rec.TimeMs))
                    {
                        rec.TimeMs += rec.Time * 1000;
                        break;
                    }
                    else
                        rec.TimeMs = rec.Time * 1000;
                if ((rec.UpdateMask & 0x10000000) != 0)
                    if (!br.GetLong(out var volEx))
                        break;
                if ((rec.UpdateMask & 0x20000000) != 0)
                    if (!br.GetLong(out var volHighEx))
                        break;
                if ((rec.UpdateMask & 0x40000000) != 0)
                    if (!br.GetLong(out var volLowEx))
                        break;
                if ((rec.UpdateMask & 0x80000000) != 0)
                    if (!br.GetLong(out var volBuyEx))
                        break;
                if ((rec.UpdateMask & 0x100000000) != 0)
                    if (!br.GetLong(out var volSellEx))
                        break;
                if ((rec.UpdateMask & 0x200000000) != 0)
                    if (!br.GetLong(out var volDealsEx))
                        break;
                if ((rec.UpdateMask & 0x400000000) != 0)
                    if (!br.GetLong(out rec.PriceDelta))
                        break;
                if ((rec.UpdateMask & 0x800000000) != 0)
                    if (!br.GetLong(out rec.PriceTheta))
                        break;
                if ((rec.UpdateMask & 0x1000000000) != 0)
                    if (!br.GetLong(out rec.PriceGamma))
                        break;
                if ((rec.UpdateMask & 0x2000000000) != 0)
                    if (!br.GetLong(out rec.PriceVega))
                        break;
                if ((rec.UpdateMask & 0x4000000000) != 0)
                    if (!br.GetLong(out rec.PriceRho))
                        break;
                if ((rec.UpdateMask & 0x8000000000) != 0)
                    if (!br.GetLong(out rec.PriceOmega))
                        break;
                if ((rec.UpdateMask & 0x10000000000) != 0)
                    if (!br.GetLong(out rec.PriceSensitivity))
                        break;
                //if (rec.m_lUpdateMask & 0x10000200)
                //    rec.m_lVolume = vol * 100000000 + volEx;
                //if (rec.m_lUpdateMask & 0x20000400)
                //    rec.m_lVolumeHigh = volHigh * 100000000 + volHighEx;
                //if (rec.m_lUpdateMask & 0x40000800)
                //    rec.m_lVolumeLow = volLow * 100000000 + volLowEx;
                //if (rec.m_lUpdateMask & 0x80001000)
                //    rec.m_lBuyVolume = volBuy * 100000000 + volBuyEx;
                //if (rec.m_lUpdateMask & 0x100002000)
                //    rec.m_lSellVolume = volSell * 100000000 + volSellEx;
                //if (rec.m_lUpdateMask & 0x200020000)
                //    rec.m_lDealsVolume = volDeals * 100000000 + volDealsEx;
                br.SkipRecords(rec.UpdateMask, 0x10000000000);
                if ((rec.UpdateMask & 0x4000000000000000) != 0)
                {
                    ulong mask;
                    if (!br.GetULong(out mask))
                        break;
                    br.SkipRecords(mask, 0);
                }
                if ((rec.UpdateMask & 0x8000000000000000) != 0)
                {
                    ulong mask;
                    if (!br.GetULong(out mask))
                        break;
                    br.SkipRecords(mask, 0);
                }
                br.AlignBitPosition(1);
                SymbolInfo info;
				try
				{
                   info = QuoteClient.Symbols.GetInfo(rec.Id);
                }
				catch (Exception)
				{
                    continue;
				}
                rec.Symbol = info.Currency;
                var sum = new MarketWatch();
                sum.Symbol = rec.Symbol;
                sum.Bid = ConvertTo.LongLongToDouble(info.Digits, rec.Bid);
                sum.Ask = ConvertTo.LongLongToDouble(info.Digits, rec.Ask);
                sum.High = ConvertTo.LongLongToDouble(info.Digits, rec.BidHigh);
                sum.Low = ConvertTo.LongLongToDouble(info.Digits, rec.BidLow);
                sum.OpenPrice = ConvertTo.LongLongToDouble(info.Digits, rec.OpenPrice);
                sum.ClosePrice = ConvertTo.LongLongToDouble(info.Digits, rec.ClosePrice);
                if(sum.Bid != 0 && sum.OpenPrice != 0)
                    sum.DailyChange = (sum.Bid - sum.OpenPrice) / sum.OpenPrice * 100;
                if(sum.Ask != 0 && sum.Bid != 0)
                    sum.Spread = (int)Math.Round((sum.Ask - sum.Bid) / info.Points);
                sum.Volume = rec.Volume;
                while (true)
                {
                    if (MarketWatches.TryGetValue(sum.Symbol, out var watch))
                    {
                        if (sum.Bid != 0) watch.Bid = sum.Bid;
                        if (sum.Ask != 0) watch.Ask = sum.Ask;
                        if (sum.High != 0) watch.High = sum.High;
                        if (sum.Low != 0) watch.Low = sum.Low;
                        if (sum.OpenPrice != 0) watch.OpenPrice = sum.OpenPrice;
                        if (sum.ClosePrice != 0) watch.ClosePrice = sum.ClosePrice;
                        if (sum.DailyChange != 0) watch.DailyChange = sum.DailyChange;
                        if (sum.Spread != 0) watch.Spread = sum.Spread;
                        if(sum.Volume != 0) watch.Volume = sum.Volume;
                    }
                    else if (!MarketWatches.TryAdd(sum.Symbol, sum))
                        continue;
                    break;
                }
                var r = new TickRec();
                r.Id = rec.Id;
                r.Ask = rec.Ask;
                r.Bid = rec.Bid;
                r.Last = rec.Last;
                r.Time = rec.Time;
                r.TimeMs = rec.TimeMs;
                r.UpdateMask = rec.UpdateMask;
                if (!LastQuotes.TryGetValue(info.Currency, out var lastQuote))
                {
                    lastQuote = new Quote
                    {
                        Symbol = info.Currency
                    };
                    if(!LastQuotes.TryAdd(info.Currency, lastQuote))
                        LastQuotes.TryGetValue(info.Currency, out lastQuote);
                }
                RecToQuote(r, lastQuote);
                string serverName = QuoteClient.ClusterSummary?.ServerName;
                if (serverName != null)
                {
                    LastQuotesByServerName.TryAdd(serverName, new ConcurrentDictionary<string, Quote>());
                    if (LastQuotesByServerName.TryGetValue(serverName, out var quotes))
                        quotes[lastQuote.Symbol] = lastQuote;
                }
                if (Quotes.TryGetValue(info.Currency, out var quote))
                {
                    if (quote == null)
                    {
                        quote = new Quote
                        {
                            Symbol = info.Currency
                        };
                        Quotes.TryUpdate(info.Currency, quote, null);
                    }
                }
                else
                    continue;
                RecToQuote(r, quote);
				var res = RoundBidAsk(quote);
                if (res != null)
                    if (Math.Round(res.Bid, 8) != 0)
                        if (Math.Round(res.Ask, 8) != 0)
                            QuoteClient.OnQuoteCall(res);
			}
        }

        public void Parse(InBuf buf)
        {
            var bytes = buf.ToBytes();
            BitReaderQuotes br = new BitReaderQuotes(bytes, bytes.Length * 8);
            br.Initialize(2, 3);
            while (true)
            {
                TickRec rec = new TickRec();
                if (!br.GetInt(out rec.Id))
                    break;
                if (!br.GetLong(out rec.Time))
                    break;
                if (!br.GetULong(out rec.UpdateMask))
                    break;
                if ((rec.UpdateMask & 1) != 0)
                    if (!br.GetLong(out rec.Bid))
                        break;
                if ((rec.UpdateMask & 2) != 0)
                    if (!br.GetLong(out rec.Ask))
                        break;
                if ((rec.UpdateMask & 4) != 0)
                    if (!br.GetLong(out rec.Last))
                        break;
                ulong volume = 0;
                if ((rec.UpdateMask & 8) != 0)
                    if (!br.GetULong(out volume))
                        break;
                if ((rec.UpdateMask & 0x10) != 0)
                    if (!br.GetLong(out rec.s3C))
                        break;
                if ((rec.UpdateMask & 0x20) != 0)
                    if (!br.GetShort(out rec.BankId))
                        break;
                    else
                        rec.BankId = -1;
                if ((rec.UpdateMask & 0x40) != 0)
                {
                    if (!br.GetLong(out rec.TimeMs))
                        break;
                    rec.TimeMs += rec.Time * 1000;
                }
                else
                    rec.TimeMs = rec.Time * 1000;
                if ((rec.UpdateMask & 0x80) != 0) //if symbol delayеd show quote to client, else calc profit  
                    if (!br.GetLong(out rec.s44))
                        break;
                ulong volumeEx = 0;
                if ((rec.UpdateMask & 0x100) != 0)
                    if (!br.GetULong(out volumeEx))
                        break;
                if ((rec.UpdateMask & 0x108) != 0)
                    rec.Volume = volume * 100000000 + volumeEx;
                br.SkipRecords(rec.UpdateMask, 0x100);
                br.AlignBitPosition(1);
                var info = QuoteClient.Symbols.GetInfo(rec.Id);
                if (rec.s44 == 0 && QuoteClient.ServerDetails.Key.ServerName.Contains("AmanaCapital"))
                {
#if DELAYED_SYMBOLS
                    lock (QuoteClient.DelayedSymbols)
                    {
                        if (QuoteClient.DelayedSymbols.ContainsKey(info.Currency))
                        {
                            var q = QuoteClient.DelayedSymbols[info.Currency];
                            if (q == null)
                            {
                                q = new Quote
                                {
                                    Symbol = info.Currency
                                };
                                QuoteClient.DelayedSymbols[q.Symbol] = q;
                            }
                            RecToQuote(rec, q);
                            continue;
                        }       
                    }
#endif
                }
                if (!LastQuotes.TryGetValue(info.Currency, out var lastQuote))
                {
                    lastQuote = new Quote
                    {
                        Symbol = info.Currency
                    };
                    if (!LastQuotes.TryAdd(info.Currency, lastQuote))
                        LastQuotes.TryGetValue(info.Currency, out lastQuote);
                }
                RecToQuote(rec, lastQuote);
                string serverName = QuoteClient.ClusterSummary?.ServerName;
                if (serverName != null)
                {
                    LastQuotesByServerName.TryAdd(serverName, new ConcurrentDictionary<string, Quote>());
                    if (LastQuotesByServerName.TryGetValue(serverName, out var quotes))
                        quotes[lastQuote.Symbol] = lastQuote;
                }
                if (Quotes.TryGetValue(info.Currency, out var quote))
                {
                    if (quote == null)
                    {
                        quote = new Quote
                        {
                            Symbol = info.Currency
                        };
                        Quotes.TryUpdate(info.Currency, quote, null);
                    }
                }
                else
                    continue;
                RecToQuote(rec, quote);
                var res = RoundBidAsk(quote);
                if (res != null)
                    if (Math.Round(res.Bid, 8) != 0)
                        if (Math.Round(res.Ask, 8) != 0)
                            QuoteClient.OnQuoteCall(res);
            }
        }

        Quote RoundBidAsk(Quote quote)
        {
            if (quote == null)
                return null;
            var info = QuoteClient.Symbols.GetInfo(quote.Symbol);
            var group = QuoteClient.Symbols.GetGroup(quote.Symbol);
            var deviation = group.DeviationRate;
            var round = group.RoundRate;
            var points = info.Points;
            var digits = info.Digits;
            if (deviation == 0)
                return new Quote
                {
                    Symbol = quote.Symbol,
                    Bid = quote.Bid,
                    Ask = quote.Ask,
                    Time = quote.Time,
                    Last = quote.Last
                };
            if (quote.Bid == 0 || quote.Ask == 0)
                return null;
            double v = Math.Round((double)deviation + (quote.Ask - quote.Bid) / points, 0);
            int var = deviation - deviation / 2;
            if (deviation > 0)
                var -= round;
            else
                var += round;
            double bid = Math.Round(quote.Bid - (double)var * points, digits);
            double ask = Math.Round(bid + v * points, digits);
            var res = new Quote
            {
                Symbol = quote.Symbol,
                Bid = bid,
                Ask = ask,
                Time = quote.Time,
                Last = quote.Last
            };
            return res;
        }

        //double Round(double value, int digits)
        //{
        //    double b =  LL2DBL(DBL2LL(value));
        //    if (b == -923372036854776)
        //        return b;
        //    digits = Math.Min(digits, 11);
        //    long m = DBL2LL((value - b) * s_tblDegreeP[digits] + ((value > 0) ? 0.5 : -0.5));
        //    return b + LL2DBL(m) / s_tblDegreeP[digits];
        //}
        //double[] s_tblDegreeP = new double[]{ 1.0, 1.0e1, 1.0e2, 1.0e3, 1.0e4, 1.0e5, 1.0e6, 1.0e7, 1.0e8, 1.0e9, 1.0e10, 1.0e11, 1.0e12, 1.0e13, 1.0e14, 1.0e15 };

        //double LL2DBL(long value)
        //{
        //    return (double)value;
        //}

        //long DBL2LL(double value)
        //{
        //    return (long)value;
        //}

        void RecToQuote(TickRec rec, Quote quote)
        {
            SymbolInfo info = QuoteClient.Symbols.GetInfo(rec.Id);
            quote.Time = ConvertTo.DateTimeMs(rec.TimeMs);
            quote.BankId = rec.BankId;
            quote.UpdateMask = rec.UpdateMask;
            //if ((rec.UpdateMask & 1)!=0)
            if(rec.Bid!=0)
                quote.Bid = ConvertTo.LongLongToDouble(info.Digits, rec.Bid);
            //if ((rec.UpdateMask & 2)!=0)
            if (rec.Ask != 0)
                quote.Ask = ConvertTo.LongLongToDouble(info.Digits, rec.Ask);
            //if ((rec.UpdateMask & 4) != 0)
            if (rec.Last != 0)
                quote.Last = ConvertTo.LongLongToDouble(info.Digits, rec.Last);
            //if ((rec.UpdateMask & 8) != 0)
            if (rec.Volume != 0)
                quote.Volume = rec.Volume;
            //if ((rec.UpdateMask & 8) != 0x80)
            //    quote.s44 = rec.s44;
        }
    }
}
