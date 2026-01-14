using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    class AccountLoader
    {
        Logger Log;
        MT5API QuoteClient;
        CmdHandler CmdHandler;
        Connection Connection;

        internal AccountLoader(MT5API qc, CmdHandler cmdHandler, Connection connection)
        {
            Log = new Logger(this);
            QuoteClient = qc;
            CmdHandler = cmdHandler;
            Connection = connection;
            QuoteClient.Symbols.Groups = new ConcurrentDictionary<string, SymGroup>();
			QuoteClient.Symbols.Sessions = new ConcurrentDictionary<string, SymbolSessions>();
		}

        internal void Parse(InBufAccLoad buf)
        {
            try
            {
                var status = (Msg)buf.Int();
                if (status != Msg.DONE)
                    throw new Exception("Account info parse status = " + status);
                var openOrders = new ConcurrentDictionary<long, Order>();
                while (buf.hasData)
                {
                    var cmd = buf.Byte();
                    int n = 0;
                    if (cmd == 0) // StoickFX temporary fix for extra zeros in the stream
                        while (cmd != 0x17)
                        {
                            cmd = buf.Byte();
                            n++;
                        }
                    switch (cmd)
                    {
                        case 0x07:                              //symbols
                            LoadSymbols(buf);
							LoadSymbolSets(buf);
							break;
                        case 0x11:                              //tickers
                            LoadTickers(buf);
                            break;
                        case 0x17:                              //server
                            QuoteClient.ServerDetails = LoadServer(buf);
                            break;
                        case 0x18:                              //mail recepients
                            var mr = LoadMailRecepients(buf);
                            break;
                        case 0x1F:                              //order
                            OpenedClosedOrders.Add(LoadOrders(buf), QuoteClient, openOrders);
                            break;
                        case 0x24:                              //deal
                            OpenedClosedOrders.Add(LoadDeals(buf), QuoteClient, openOrders);
                            break;
                        case 0x25:                              //account
                            QuoteClient.Account= LoadAccount(buf);
                            break;
                        case 0x28:                              //spreads
                            LoadSpreads(buf);
                            break;
						case 0x67:                              //subscriptions
							LoadSubscriptions(buf);
							break;
						case 0x69:                              //subscription categories
							int size = buf.Int();
                            buf.Bytes(0x80 * size);
                            break;
						case 0x78:                              //payments
                            LoadPayments(buf);
							break;
						case 0x79:                              //payments
							LoadPayments2(buf);
							break;
						case 0x80:                              //payments
							LoadPayments2(buf);
							break;
						case 0x84:
                            LoadSomeAccountData(buf); 
							break;
                        case 0x8B:
                            UpdateSignalSubscription(buf);
                            break;
                        default:
                            //br = true;
                            break;
                    }
                    //if (br)
                       // break;
                }
                QuoteClient.Orders.Opened = openOrders;
                CmdHandler.GotAccountInfo = true;
                CmdHandler.GotAccountInfoTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                CmdHandler.AccountLoaderException = ex;
                QuoteClient.OnConnectCall(ex, ConnectProgress.Exception);
            }
            finally
            {
                buf.Buf = null;
            }
        }

        void UpdateSignalSubscription(InBuf buf)
        {
            buf.Bytes(16);
        }

        private void LoadPayments2(InBuf buf)
		{
			var num = buf.Int();
            for (int i = 0; i < num; i++)
            {
				buf.Bytes(20);          //vPaymentRec
				int size = buf.Int();   //vPaymentRec
				buf.Bytes(104);         //vPaymentRec
				buf.Bytes(size);        
			}
		}

		private void LoadPayments3(InBuf buf)
		{
			var num = buf.Int();
			for (int i = 0; i < num; i++)
			{
				buf.Bytes(20);          //vPaymentRec
				int size = buf.Int();   //vPaymentRec
				buf.Bytes(104);         //vPaymentRec
				buf.Bytes(size);
			}
		}

		private void LoadSomeAccountData(InBuf buf)
		{
			buf.Bytes(3084);
			var num = buf.Int();
			for (int i = 0; i < num; i++)
				buf.Bytes(1288);
		}

		private void LoadSubscriptions(InBuf buf)
		{
			Msg status = (Msg)buf.Int();
			if (status == Msg.OK)
				return;
			if (status != Msg.DONE)
				throw new Exception(status.ToString());
			int num = buf.Int();
			for (int i = 0; i < num; i++)
            {
				buf.Bytes(1240); //vSubscriptionInfo
				int size = buf.Int();
				buf.Bytes(size);
				var count = buf.Int();
				for (int j = 0; j < count; j++)
					buf.Bytes(256);
				count = buf.Int();
				for (int j = 0; j < count; j++)
					buf.Bytes(256);
				count = buf.Int();
				for (int j = 0; j < count; j++)
					buf.Bytes(292);
				count = buf.Int();
				for (int j = 0; j < count; j++)
					buf.Bytes(292);
			}
		}

		private void LoadPayments(InBuf buf)
		{
			int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                buf.Bytes(776);
                int size = buf.Int();
                if (size == 0)
                    size = buf.Int();
                buf.Bytes(size);
                var count = buf.Int();
                for (int j = 0; j < count; j++)
                    buf.Bytes(528);
                count = buf.Int();
                for (int j = 0; j < count; j++)
                    buf.Bytes(208);
                count = buf.Int();
                for (int j = 0; j < count; j++)
                    buf.Bytes(112);
            }
		}

		private void LoadSpreads(InBuf buf)
        {
            Log.trace("LoadSpreads");
            Msg status = (Msg)buf.Int();
            if (status == Msg.OK)
                return;
            if (status != Msg.DONE)
                throw new Exception(status.ToString());
            int num = buf.Int();
            for (int i = 0; i < num; i++)
                LoadSpread(buf);
            LoadRemoveList(buf);
        }

        private void LoadSpread(InBuf buf)
        {
            var si = buf.Struct<SpreadInfo>();
            int num = buf.Int();
            SpreadData[] buy = new SpreadData[num];
            for (int i = 0; i < num; i++)
                buy[i] = buf.Struct<SpreadData>();
            num = buf.Int();
            SpreadData[] sell = new SpreadData[num];
            for (int i = 0; i < num; i++)
                sell[i] = buf.Struct<SpreadData>();
        }

        private AccountRec LoadAccount(InBuf buf)
        {
          return buf.Struct<AccountRec>();
        }

        private List<DealInternal> LoadDeals(InBuf buf)
        {
            Log.trace("LoadDeals");
            var updateID = buf.Int();
            int num = buf.Int();
            var list = new List<DealInternal>();
            for (int i = 0; i < num; i++)
            {
                if (Connection.TradeBuild < 1891)
                    throw new NotImplementedException();
                var d = buf.Struct<DealInternal>();
                list.Add(d);
            }
            return list;
        }

        private List<OrderInternal> LoadOrders(InBuf buf)
        {
            Log.trace("LoadOrders");
            var updateID = buf.Int();
            int num = buf.Int();
            var list = new List<OrderInternal>();
            for (int i = 0; i < num; i++)
            {
                if (Connection.TradeBuild < 1891)
                    throw new NotImplementedException();
                var o = buf.Struct<OrderInternal>();
                list.Add(o);
            }
            return list;
        }

        private MailRecipient[] LoadMailRecepients(InBuf buf)
        {
            Log.trace("LoadMailRecepients");
            return buf.Array<MailRecipient>();
        }

        private KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]> LoadServer(InBuf buf)
        {
            Log.trace("LoadServer");
            var sr = buf.Struct<ServerRec>();
            int num = buf.Int();
            var list = new List<KeyValuePair<AccessInfo, AddressRec[]>>();
            for (int i = 0; i < num; i++)
                list.Add(LoadAccess(buf));
            return new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>(sr, list.ToArray());
        }


        private KeyValuePair<AccessInfo, AddressRec[]> LoadAccess(InBuf buf)
        {
            var ai = buf.Struct<AccessInfo>();
            int num = buf.Int();
            var list = new List<AddressRec>();
            for (int i = 0; i < num; i++)
                list.Add(buf.Struct<AddressRec>());
            return new KeyValuePair<AccessInfo, AddressRec[]>(ai, list.ToArray());
        }

        private void LoadTickers(InBuf buf)
        {
            Log.trace("LoadTickers");
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                if (Connection.SymBuild <= 1036)
                    throw new NotImplementedException();
                else
                {
                    var ticker = buf.Struct<Ticker>();
                }
            }
        }

        private void LoadSymbols(InBuf buf)
        {
            Log.trace("LoadSymbols");
            buf.SymBuild = Connection.SymBuild;
            LoadSymBase(buf);
			if (Connection.SymBuild >= 4072)
				LoadLeveargeBase(buf); //vLeverageBaseLoad
			Msg status = (Msg)buf.Int();
            if (status == Msg.OK)
            {
                Log.trace("DeleteDuplicatedSymbols");
                //DeleteDuplicatedSymbols();
                return;
            }
            if (status != Msg.DONE)
                throw new Exception(status.ToString());
            if (Connection.SymBuild <= 1891)
                throw new NotImplementedException("SymBuild <= 1891");
            int num = buf.Int();
            QuoteClient.Symbols.Infos = new ConcurrentDictionary<string, SymbolInfo>();
            QuoteClient.Symbols.InfosById = new ConcurrentDictionary<int, SymbolInfo>();
            for (int i = 0; i < num; i++)
            {
                var si = UDT.ReadStruct<SymbolInfo>(buf);
                QuoteClient.Symbols.Infos.TryAdd(si.Currency, si);
                if(!QuoteClient.Symbols.InfosById.ContainsKey(si.Id)) 
                    QuoteClient.Symbols.InfosById.TryAdd(si.Id, si);
                var gr = UDT.ReadStruct<SymGroup>(buf);
                QuoteClient.Symbols.Groups.TryAdd(si.Currency, gr);
                QuoteClient.Symbols.Sessions.TryAdd(si.Currency, LoadSessions(buf));
                var sc54 = UDT.ReadStruct<SymTicks>(buf);
            }
            //if (QuoteClient.Symbols.Groups.Values.Count < 1000 && QuoteClient.Symbols.SymGroups.Length < 100)
            //    foreach (var main in QuoteClient.Symbols.Groups.Values)
            //    {
            //        foreach (var slave in QuoteClient.Symbols.SymGroups)
            //        {
            //            if (slave.GroupName.Contains("(") || slave.GroupName.Contains(")"))
            //                continue;
            //            if (new Regex(slave.GroupName.Replace(@".", @"\.").Replace(@"\", @"\\").Replace("*", ".*")).Matches(main.GroupName).Count > 0)
            //                main.CopyValues(slave);
            //        }
            //    }
            LoadRemoveList(buf);
        }

        void LoadRemoveList(InBuf buf)
        {
            int num = buf.Int();
            int[] ar = new int[num]; //m_SymInfo.m_nId
            for (int i = 0; i < num; i++)
                ar[i] = buf.Int();
        }

        void LoadSymbolSets(InBuf buf)
        {
            Msg status = (Msg)buf.Int();
            if (status == Msg.OK)
            {
                return;
            }
            if (status != Msg.DONE)
                throw new Exception(status.ToString());
            int num = buf.Int();
            for (int i = 0; i < num; i++)
            {
                //var size = Marshal.SizeOf(typeof(SymbolSet));
                //var bytes = buf.Bytes(size);
                var ss = UDT.ReadStruct<SymbolSet>(buf);
            }
        }

        internal static SymbolSessions LoadSessions(InBuf buf)
        {
            List<Session>[] quotes = new List<Session>[7];
            List<Session>[] trades = new List<Session>[7];
            for (int i = 0; i < 7; i++)
            {
                int num = buf.Int();
                quotes[i] = new List<Session>();
                for (int j = 0; j < num; j++)
                {
                    var s = UDT.ReadStruct<Session>(buf);
                    quotes[i].Add(s);
                }
                num = buf.Int();
                trades[i] = new List<Session>();
                for (int j = 0; j < num; j++)
                {
                    var s = UDT.ReadStruct<Session>(buf);
                    trades[i].Add(s);
                }
            }
            var ss = new SymbolSessions
            {
                Quotes = quotes,
                Trades = trades
            };
            return ss;
        }

        void LoadSymBase(InBuf buf)
        {
            var build = Connection.SymBuild;
            if (build <= 1495)
                throw new NotImplementedException("SymBuild: " + Connection.SymBuild.ToString());//return LoadBuild1495(pBufMan);
            if (build <= 1613)
                throw new NotImplementedException("SymBuild: " + Connection.SymBuild.ToString()); //return LoadBuild1613(pBufMan);
            if (build <= 1891)
                throw new NotImplementedException("SymBuild: " + Connection.SymBuild.ToString()); //return LoadBuild1891(pBufMan);
            if (build <= 2017)
            {
                LoadBuild2017(buf);
                return;
            }
            if (build <= 2124)
            {
                LoadBuild2124(buf);
                return;
            }
            if (build <= 2204)
            {
                LoadBuild2204(buf);
                return;
            }
			if (build <= 2204)
			{
				LoadBuild2204(buf);
				return;
			}
            LoadLastBuild(buf);

		}

		private void LoadLastBuild(InBuf buf)
		{
			QuoteClient.Symbols.Base = UDT.ReadStruct<SymBaseInfo>(buf);
			int num = buf.Int();
			SymGroup[] ar = new SymGroup[num];
			for (int i = 0; i < num; i++)
			{
				var gr = UDT.ReadStruct<SymGroup>(buf);
				ar[i] = gr;
			}
			QuoteClient.Symbols.SymGroups = ar;
			num = buf.Int();
			var infos = new ComissionInfo[num];
			for (int i = 0; i < num; i++)
			{
				var info = UDT.ReadStruct<ComissionInfo>(buf);
				info.Comissions = buf.Array<Comission>();
				infos[i] = info;
			}
			QuoteClient.Symbols.Comissions = infos;
		}

        private void LoadBuild2124(InBuf buf)
        {
            QuoteClient.Symbols.Base = UDT.ReadStruct<SymBaseInfo>(buf);
            int num = buf.Int();
            SymGroup[] ar = new SymGroup[num];
            for (int i = 0; i < num; i++)
            {
                var gr = UDT.ReadStruct<SymGroup>(buf);
                ar[i] = gr;
            }
            QuoteClient.Symbols.SymGroups = ar;
            num = buf.Int();
            if (num > 0)
                Log.trace("vSymXX count > 0");
            for (int i = 0; i < num; i++)
                LoadSymXX(buf);
        }

        private void LoadBuild2204(InBuf buf)
        {
            QuoteClient.Symbols.Base = UDT.ReadStruct<SymBaseInfo>(buf);
            int num = buf.Int();
            SymGroup[] ar = new SymGroup[num];
            for (int i = 0; i < num; i++)
            {
                var gr = UDT.ReadStruct<SymGroup>(buf);
                ar[i] = gr;
            }
            QuoteClient.Symbols.SymGroups = ar;
            num = buf.Int();
            if (num > 0)
                Log.trace("vSymXX count > 0");
            for (int i = 0; i < num; i++)
                LoadSymXX(buf);
        }

        private void LoadBuild2017(InBuf buf)
        {
            QuoteClient.Symbols.Base = UDT.ReadStruct<SymBaseInfo>(buf);
            int num = buf.Int();
            SymGroup[] ar = new SymGroup[num];
            for (int i = 0; i < num; i++)
            {
                var gr = UDT.ReadStruct<SymGroup>(buf);
                ar[i] = gr;
            }
            QuoteClient.Symbols.SymGroups = ar;
            num = buf.Int();
            if (num > 0)
                Log.trace("vSymXX count > 0");
            for (int i = 0; i < num; i++)
                LoadSymXX(buf);
        }

        void LoadSymXX(InBuf buf)
        {
            buf.Bytes(0x38C); //vSymXXInfo
            int num = buf.Int();
            for (int i = 0; i < num; i++)
                buf.Bytes(0xA0); //vSymYY
        }

		internal static void LoadLeveargeBase(InBuf buf) //vLeverageBaseLoad
		{
			buf.Bytes(656); //vLeverageBaseInfo
			int num = buf.Int();
            for (int i = 0; i < num; i++) //vLeverages
			{
                buf.Bytes(932); //vLeverageInfo
				int count = buf.Int();
                for (int j = 0; j < count; j++)
					buf.Bytes(160); //vLeverage
			}
		}
	}
}
