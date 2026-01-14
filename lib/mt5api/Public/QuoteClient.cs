//#define DELAYED_SYMBOLS

using mtapi.mt5.Internal;
using mtapi.mt5.Struct;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    /// <summary>
    /// Main class to trade and get data
    /// </summary>
    public partial class MT5API
    {
        public string LoginIdPath = "http://loginid-mt5.mtapi.io";
        public int LoginIdWebServerTimeout = 10000;
        public string ApiKey = "5a5a59f3-c4a1-4150-91b0-f823427ad3ca";
#if DELAYED_SYMBOLS
        internal Dictionary<string, Quote> DelayedSymbols =   new Dictionary<string, Quote>() { { "EURUSD.m", null } };
#endif

        public string[] ExLoginManagers;
        public string[] ExLoginProxies;
        /// <summary>
        /// One time password
        /// </summary>
        public string OtpPassword { get; set; } = null;

        ///// <summary>
        ///// One time key
        ///// </summary>
        //public string OtpKey { get; set; } = null;

        /// <summary>
        /// Time when connected to server
        /// </summary>
        public DateTime ConnectTime { get; internal set; }

        /// <summary>
        /// Dissconnect on symbol update to connect again and refresh data
        /// </summary>
        public bool DisconnectOnSymbolUpdate { get; set; } = false;

        /// <summary>
        /// If you need so subscribe all symbols on each mt4 put 'true' to speed up application
        /// </summary>
        public bool ProcessServerMessagesInThread { get; set; } = false;

        /// <summary>
        /// For user purposes
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Certificate *.pfx file
        /// </summary>
        public byte[] PfxFile { get; set; }

        /// <summary>
        /// Pfx file password
        /// </summary>
        public string PfxFilePassword { get; set; }

        /// <summary>
        /// Allow to connect to mt server located in local netwrok
        /// </summary>
        public bool DisallowLocalConnections = false;

        /// <summary>
        /// Use connect in task instead of thread
        /// </summary>
        public bool UseConnectTask = true;

        public bool ProxyEnable;
        public string ProxyHost;
        public int ProxyPort;
        public string ProxyUser;
        public string ProxyPassword;
        public ProxyTypes ProxyType;

        private byte[] _CommonKey;
        public byte[] CommonKey
        {
            get
            {
                return _CommonKey;
            }
            set
            {
                if (value != null)
                    if (value.Length != 15)
                        throw new ArgumentException("CommonKey must be 15 bytes length");
                _CommonKey = value;
            }
        }


        public int GetQuoteTimeoutMs = 10000;
        public int ProcessEventTimeoutMs = 30000;
        public int DownloadOrderHistoryTimeout = 30000;

        /// <summary>
        /// Symbols information
        /// </summary>
        public readonly Symbols Symbols;
        //// <summary>
        /// New quote
        /// </summary>   
		public event OnQuote OnQuote;
        //// <summary>
        /// Order history
        /// </summary>   
        public event OnOrderHistory OnOrderHistory;
        //// <summary>
        /// Order update notification from server
        /// </summary>
        public event OnOrderUpdate OnOrderUpdate;
        //// <summary>
        /// Symbol update notification
        /// </summary>
        public event OnSymbolUpdate OnSymbolUpdate;
        //// <summary>
        /// Connect progress notification
        /// </summary>
        public event OnConnectProgress OnConnectProgress;
        //// <summary>
        /// Symbols update notification
        /// </summary>
        public event OnSymbolsUpdate OnSymbolsUpdate;
        /// <summary>
        /// Open/close progress of the order before ticket number assign.
        /// </summary>
        public event OnOrderProgress OnOrderProgress;
        /// <summary>
        /// Quote history event. Use RequestQuoteHistory to request history. 
        /// </summary>
        public event OnQuoteHistory OnQuoteHistory;
        /// <summary>
        /// Tick history event. Use RequestQuoteHistory to request history. 
        /// </summary>
        public event OnTickHistory OnTickHistory;
        /// <summary>
        /// Mail inbox. 
        /// </summary>
        public event OnMail OnMail;
        /// <summary>
        /// Mail inbox. 
        /// </summary>
        public event OnOrderBook OnOrderBook;


        /// <summary>
        /// Last quote time, refreshing goes with using incoming quotes. 
        /// </summary>
        public DateTime LastQuoteTime { get; internal set; }
        /// <summary>
		/// Server time, refreshing goes with using incoming quotes. 
		/// </summary>
		public DateTime ServerTime
        {
            get
            {
                return DateTime.UtcNow.AddMinutes(ServerDetails.Key.TimeZone).AddHours(ServerDetails.Key.DST);
            }
        }

        private byte[] HardId;
        /// <summary>
        /// Terminal ID that shows in MT5 Server
        /// </summary>
		public byte[] HardwareId
        {
            get
            {
                return HardId;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length != 16)
                        throw new ArgumentException("HardwaredId should be 16 bytes length");
                    value[0] = 0;
                    for (int i = 1; i < 16; i++)
                        value[0] += value[i];
                }
                HardId = value;
            }
        }
        /// <summary>
        /// Offset from UTC in minutes
        /// </summary>
        public int ServerTimeZoneInMinutes
        {
            get
            {
                return ServerDetails.Key.TimeZone + ServerDetails.Key.DST * 60;
            }
        }

        /// <summary>
        /// When true: if you call Connect during connection process by another thread exception would be thrown. 
        /// When false: if you call Connect during connection process your thread will be waiting for connection process ending.
        /// </summary>
        //public bool ThrowExceptionIfCallConnectDuringConnecting { get; set; } = false;

        internal ConcurrentDictionary<long, int> RequestIds = new ConcurrentDictionary<long, int>();

        /// <summary>
        ///  Force reconnect if no quotes for some number of milliseconds.
        /// </summary>
        public int NoServerMessagesTimeout = 30000;


        /// <summary>
        /// Fresh server details recieved from server during connect.
        /// </summary>
        internal KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]> ServerDetails { get; set; } = new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>();

        /// <summary>
        /// Cluster general information.
        /// </summary>
        public ServerRec ClusterSummary {
            get
            {
                if (ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
                    return null;
                return ServerDetails.Key;
            }
        }
        /// <summary>
        /// Cluster members
        /// </summary>
        public Dictionary<AccessInfo, AddressRec[]> ClusterMembers
        {
            get
            {
                if (ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
                    return null;
                var res = new Dictionary<AccessInfo, AddressRec[]>();
                foreach (var member in ServerDetails.Value)
                    res.Add(member.Key, member.Value);
                return res;
            }
        }
        /// <summary>
        /// Account currency
        /// </summary>
        public string AccountCurrency
        {
            get
            {
                return Symbols.Base.Currency;
            }
        }

        public short ServerBuild
        {
            get
            {
                if (Connection == null)
                    throw new ConnectException("Not connected");
                return Connection.TradeBuild;
            }
        }

        /// <summary>
        /// Company name
        /// </summary>
        public string AccountCompanyName
        {
            get
            {
                if (Symbols.Base == null)
                    throw new ConnectException("Not connected");
                return Symbols.Base.CompanyName;
            }
        }
        /// <summary>
        /// Netting or hedging
        /// </summary>
        public AccMethod AccountMethod
        {
            get
            {
                if (Symbols.Base == null)
                    throw new ConnectException("Not connected");
                return Symbols.Base.AccMethod;
            }
        }

        /// <summary>
        /// Account profit
        /// </summary>

        public double AccountProfit
        {
            get
            {
                return _AccountProfit;
            }
        }

        private double _AccountProfit;


        /// <summary>
        /// Account used margin
        /// </summary>
        public double AccountMargin
        {
            get
            {
                return _AccountMargin;
            }
        }
        double _AccountMargin;

        /// <summary>
        /// Margin level
        /// </summary>
        public double MarginLevel
        {
            get
            {
                if (AccountMargin == 0)
                    return 0;
                else
                    return AccountEquity / AccountMargin * 100;
            }

        }

        /// <summary>
        /// Account free margin
        /// </summary>
        public double AccountFreeMargin
        {
            get
            { return this.AccountEquity - AccountMargin; }
        }

        /// <summary>
        /// Account equity.
        /// </summary>
        public double AccountEquity
        {
            get
            {
                return Account.Balance + AccountProfit + Account.Credit;
            }
        }
        /// <summary>
		/// Check connection state.
		/// </summary>
		public bool Connected
        {
            get
            {
                var cmdHandler = CmdHandler;
                if (cmdHandler == null)
                    return false;
                if (cmdHandler.GotAccountInfo == false)
                    return false;
                if (cmdHandler.Stop)
                    return false;
                var connection = Connection;
                if (connection == null)
                    return false;
                var sock = connection.Sock;
                if (sock == null)
                    return false;
                if (!sock.Connected)
                    return false;
                if (DateTime.Now.Subtract(LastServerMessageTime).TotalMilliseconds > NoServerMessagesTimeout)
                    return false;
                return cmdHandler.Running;
            }
        }


        /// <summary>
		/// Check subscribe trading instrument.
		/// </summary>
		/// <param name="symbol">Symbol for trading.</param>
		public bool IsSubscribed(string symbol)
        {
            return Subscriber.Subscribed(symbol);
        }


        /// <summary>
        /// Local time of last server message.
        /// </summary>
        public DateTime LastServerMessageTime = DateTime.Now;

        internal Logger Log;

        internal CmdHandler CmdHandler;
        internal Connection Connection;
        internal Subscriber Subscriber;
        internal OpenedClosedOrders Orders;
        internal OrderHistory OrderHistory;
        internal Mail Mail;
        internal OrderBook OrderBook;
        //internal Profit Profit;
        internal OrderProfit OrderProfit;

        internal Connector Connector;

        internal ConnectorTask ConnectorTask;

        /// <summary>
        /// Host
        /// </summary>
        public string Server;
        /// <summary>
        /// Account number
        /// </summary>
        public ulong User { get; set; }
        /// <summary>
        /// Account Password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Build number
        /// </summary>
        public int Build { get; set; }
        /// <summary>
        /// Read servers.dat
        /// </summary>
        /// <param name="path">Path to servers.dat</param>
        /// <returns></returns>
        public static Server[] LoadServersDat(string path)
        {
            return new ServersDatLoader().Load(path);
        }

        /// <summary>
        /// Read servers.dat
        /// </summary>
        /// <param name="bytes">servers.dat as byte array</param>
        /// <returns></returns>
        public static Server[] LoadServersDat(byte[] bytes)
        {
            return new ServersDatLoader().Load(bytes);
        }

        /// <summary>
        /// Save servers.dat
        /// </summary>
        /// <param name="bytes">servers.dat as byte array</param>
        /// <returns></returns>
        public static byte[] SaveServersDat(Server[] servers)
        {
            var header = new DatHeader();
            header.ObjNumber = servers.Length;
            return new ServersDatSaver().Save(servers, header);
        }

        /// <summary>
        /// Parameterless construcotor for desiarization
        /// </summary>
        public MT5API()
        {
            Trial.check(ApiKey);
            Log = new Logger(this);
            Subscriber = new Subscriber(this, Log);
            Orders = new OpenedClosedOrders(this, Log);
            OrderHistory = new OrderHistory(this, Log);
            OrderProfit = new OrderProfit(this);
            Connector = new Connector(this);
            ConnectorTask = new ConnectorTask(this);
            Mail = new Mail(this);
            OrderBook = new OrderBook(this);
            Symbols = new Symbols(this);
        }

        /// <summary>
        /// Main construcotr
        /// </summary>
        /// <param name="user"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        public MT5API(ulong user, string password, string host, int port,
            byte[] pfxFile = null, string pfxFilePassword = null)
        {
            Trial.check(ApiKey);
            Log = new Logger(this);
            User = user;
            Password = password;
            Host = host;
            Port = port;
            PfxFile = pfxFile;
            PfxFilePassword = pfxFilePassword;
            Subscriber = new Subscriber(this, Log);
            Orders = new OpenedClosedOrders(this, Log);
            OrderHistory = new OrderHistory(this, Log);
            OrderProfit = new OrderProfit(this);
            ConnectorTask = new ConnectorTask(this);
            Connector = new Connector(this);
            Mail = new Mail(this);
            OrderBook = new OrderBook(this);
            Symbols = new Symbols(this);
        }

        /// <summary>
        /// Main constructor to connect by server name
        /// </summary>
        /// <param name="user"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        public MT5API(ulong user, string password, string server, byte[] pfxFile = null, string pfxFilePassword = null)
        {
            Trial.check(ApiKey);
            Log = new Logger(this);
            User = user;
            Password = password;
            if (server == null)
                throw new ArgumentNullException("Server parameter is null");
            server = server.Trim();
            IList<Broker.Company> brokers = null;
            if (Broker.SearchHistory == null)
                Broker.SearchHistory = new ConcurrentDictionary<string, KeyValuePair<IList<Broker.Company>, DateTime>>();
            if (Broker.SearchHistory.TryGetValue(server, out var res))
                if (DateTime.Now.Subtract(res.Value).TotalHours < 4)
                    brokers = res.Key;

            if (brokers == null)
                try
                {
                    var task = Broker.SearchAsync(server);
                    if (!task.Wait(10000))
                        throw new TimeoutException("Broker search timeout");
                    brokers = task.Result;
                    if (brokers == null)
                        throw new NullReferenceException("Broker.SearchAsync returned null");
                    Broker.SearchHistory[server] = new KeyValuePair<IList<Broker.Company>, DateTime>(brokers, DateTime.Now);
                }
                catch (Exception)
                {
                    try
                    {
                        brokers = Broker.SearchMQ(server);
                        if (brokers == null)
                            throw new NullReferenceException("Broker.SearchMQ returned null");
                        Broker.SearchHistory[server] = new KeyValuePair<IList<Broker.Company>, DateTime>(brokers, DateTime.Now);
                    }
                    catch (Exception)
                    {
                        if (Broker.SearchHistory.TryGetValue(server, out var result))
                            brokers = result.Key;
                        else
                            throw new ServerNotFoundException("Server not found: " + server);
                    }
                }
            if (brokers == null)
                throw new ServerNotFoundException("Server not found by /Search and /SearchMQ: " + server);
            KeyValuePair<AccessInfo, AddressRec[]>[] addresses = null;
            foreach (var broker in brokers)
            {
                if (broker?.results == null) 
                    continue;  
                foreach (var item in broker.results)
                    if (item.name.ToLower() == server.Trim().ToLower())
                    {
                        addresses = new KeyValuePair<AccessInfo, AddressRec[]>[item.access.Count];
                        for (int i = 0; i < item.access.Count; i++)
                            addresses[i] = new KeyValuePair<AccessInfo, AddressRec[]>(new AccessInfo(), new AddressRec[1] { new AddressRec() { Address = item.access[i] } });
                    }
            }
            if (addresses == null)
                throw new ServerNotFoundException("Server not found: " + server);
            ServerDetails = new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>(new ServerRec(), addresses);
            PfxFile = pfxFile;
            PfxFilePassword = pfxFilePassword;
            Subscriber = new Subscriber(this, Log);
            Orders = new OpenedClosedOrders(this, Log);
            OrderHistory = new OrderHistory(this, Log);
            OrderProfit = new OrderProfit(this);
            Connector = new Connector(this);
            ConnectorTask = new ConnectorTask(this);
            Mail = new Mail(this);
            OrderBook = new OrderBook(this);
            Symbols = new Symbols(this);
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="user"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        public MT5API(ulong user, string password, string host, int port)
        {
            Trial.check(ApiKey);
            Log = new Logger(this);
            User = user;
            Password = password;
            Host = host;
            Port = port;
            Subscriber = new Subscriber(this, Log);
            Orders = new OpenedClosedOrders(this, Log);
            OrderHistory = new OrderHistory(this, Log);
            OrderProfit = new OrderProfit(this);
            Mail = new Mail(this);
            Connector = new Connector(this);
            ConnectorTask = new ConnectorTask(this);
            OrderBook = new OrderBook(this);
            Symbols = new Symbols(this);
        }

        /// <summary>
        /// Main construcotr
        /// </summary>
        /// <param name="user"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        public MT5API(ulong user, string password, string host, int port, string proxyHost, int proxyPort,
            string proxyUser, string proxyPassword, ProxyTypes type)
        {
            Trial.check(ApiKey);
            Log = new Logger(this);
            //Server = server;
            User = user;
            Password = password;
            Host = host;
            Port = port;
            ProxyUser = proxyUser;
            ProxyPassword = proxyPassword;
            ProxyHost = proxyHost;
            ProxyPort = proxyPort;
            ProxyType = type;
            ProxyEnable = true;
            Connection = new Connection(this);
            Subscriber = new Subscriber(this, Log);
            Orders = new OpenedClosedOrders(this, Log);
            OrderHistory = new OrderHistory(this, Log);
            //Workaround = new Workaround(this);
            OrderProfit = new OrderProfit(this);
            Connector = new Connector(this);
            ConnectorTask = new ConnectorTask(this);
            Mail = new Mail(this);
            OrderBook = new OrderBook(this);
            Symbols = new Symbols(this);
        }


        /// <summary>
        /// Contruct size
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns></returns>
        public double GetContractSize(string symbol)
        {
            return Symbols.GetInfo(symbol).ContractSize;
        }

        /// <summary>
        /// Reuest closed orders.
        /// </summary>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <returns>Array of orders int OnOrderHistory event.</returns>
        public void RequestPendingOrderHistory(DateTime from, DateTime to)
        {
            Log.trace("RequestPendingOrderHistory");
            if (DownloadOrderHistoryRunning.TryGetValue(0, out _))
                throw new Exception("Download Order History Running");
            if (!Connected)
                throw new Exception("Not connected");
            OrderHistory.RequestPending(from, to).Wait();
        }

        /// <summary>
        /// Reuest closed orders.
        /// </summary>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <returns>Array of orders int OnOrderHistory event.</returns>
        public void RequestOrderHistory(DateTime from, DateTime to)
        {
            Log.trace("RequestOrderHistory");
            if (DownloadOrderHistoryRunning.TryGetValue(0, out _))
                throw new Exception("Download Order History Running");
            if (!Connected)
                throw new Exception("Not connected");
            OrderHistory.Request(from, to, null, null).Wait();
        }

        /// <summary>
        /// Reuest closed orders for specified month.
        /// </summary>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <returns>Array of orders int OnOrderHistory event.</returns>
        public void RequestOrderHistory(int year, int month, List<DealInternal> partialResponse = null)
        {
            Log.trace("RequestOrderHistory");
            if (DownloadOrderHistoryRunning.TryGetValue(0, out _))
                throw new Exception("Download Order History Running");
            if (!Connected)
                Connect();
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddSeconds(-1);
            OrderHistory.Request(start, end, partialResponse.ToArray(), null).Wait();
            System.Threading.Thread.Sleep(100);
        }

        void RequestOrderHistoryInternal(DateTime from, DateTime to, DealInternal[] partiaReponse, DealInternal[] exist)
        {
            Log.trace("RequestOrderHistoryInternal");
            if (!Connected)
                throw new Exception("Not connected");
            OrderHistory.Request(from, to, partiaReponse, exist).Wait();
        }

        public void RequestDealHistory(int year, int month, List<DealInternal> partialResponse)
        {
            Log.trace("RequestDealHistoryMonth");
            if (!Connected)
                Connect();
            DateTime start = new DateTime(year, month, 1, 0, 0, 0);
            DateTime end = start.AddMonths(1).AddSeconds(-1);
            OrderHistory.Request(start, end, partialResponse.ToArray(), null).Wait();
        }

        public void RequestPendingHistory(int year, int month, List<OrderInternal> exist)
        {
            Log.trace("RequestPendingHistoryMonth");
            if (!Connected)
                Connect();
            DateTime start = new DateTime(year, month, 1, 0, 0, 0);
            DateTime end = start.AddMonths(1).AddSeconds(-1);
            if (exist == null)
                OrderHistory.RequestPending(start, end, 0, 0).Wait();
            else
            {
                long max = 0;
                foreach (var item in exist)
                {
                    if (item.HistoryTime > max)
                        max = item.HistoryTime;
                }
                OrderHistory.RequestPending(start, end, exist.Count, max).Wait();
            }
        }


        //void RequestOrderHistoryInternal(DateTime from, DateTime to, int partiallyCount = 0, long partiallyHistoryTime = 0)
        //{
        //    Log.trace("RequestOrderHistoryInternal");
        //    if (!Connected)
        //        throw new Exception("Not connected");
        //    if (partiallyCount == 0)
        //        OrderHistory.Request(from, to).Wait();
        //    else
        //        OrderHistory.Request(from, to, partiallyCount, partiallyHistoryTime).Wait();
        //}

        void RequestPendingOrderHistoryInternal(DateTime from, DateTime to)
        {
            Log.trace("RequestOrderHistoryInternal");
            if (!Connected)
                throw new Exception("Not connected");
            OrderHistory.RequestPending(from, to).Wait();
        }

        /// <summary>
        /// Reuest peinding orders for specified month.
        /// </summary>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <returns>Array of orders int OnOrderHistory event.</returns>
        public void RequestPendingOrderHistory(int year, int month, List<DealInternal> exist = null)
        {
            Log.trace("RequestOrderHistory");
            if (DownloadOrderHistoryRunning.TryGetValue(0, out _))
                throw new Exception("Download Order History Running");
            if (!Connected)
                Connect();
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddSeconds(-1);
            if (exist == null)
                OrderHistory.RequestPending(start, end).Wait();
            else
            {
                long max = 0;
                foreach (var item in exist)
                    if (item.HistoryTime > max)
                        max = item.HistoryTime;
                OrderHistory.RequestPending(start, end, exist.Count, max).Wait();
            }
            System.Threading.Thread.Sleep(100);
        }

        OrderHistoryEventArgsInternal DownloadedOrderHistory = new OrderHistoryEventArgsInternal();
        object OrdHistLock = new object();
        DateTime LastOrdHistResp;
        DateTime OrderHistoryFrom;
        DateTime OrderHistoryTo;
        bool OrderHistoryRequestedByMonth;
        int OrderHistoryRequestCount;

        /// <summary>
        /// Asynchronously polls for order history data until it's available or the operation is cancelled.
        /// </summary>
        /// <param name="from">Start time for order history query.</param>
        /// <param name="to">End time for order history query.</param>
        /// <param name="sort">Sort order (e.g., by open time, close time).</param>
        /// <param name="ascending">Whether to sort in ascending order.</param>
        /// <param name="existDeals">Optional array of existing deals to avoid duplication.</param>
        /// <param name="cancellation">Cancellation token to cancel polling.</param>
        /// <returns>Returns an <see cref="OrderHistoryEventArgs"/> once data becomes available.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled via the provided token.</exception>
        public async Task<OrderHistoryEventArgs> DownloadOrderHistoryAsync(
            DateTime from,
            DateTime to,
            OrderSort sort = OrderSort.OpenTime,
            bool ascending = true,
            DealInternal[] existDeals = null,
            CancellationToken cancellation = default)
        {
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                var res = DownloadOrderHistoryInternal(from, to, sort, ascending, existDeals);
                if (res != null)
                    return res;

                await Task.Delay(100, cancellation);
            }
        }

        /// <summary>
        /// Downaload order history.
        /// </summary>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <param name="sort">Sort by open time or close time</param>
        /// <param name="ascending">Ascending sort</param>
        /// <returns>Array of orders.</returns>
        public OrderHistoryEventArgs DownloadOrderHistory(DateTime from, DateTime to, OrderSort sort = OrderSort.OpenTime, bool ascending = true)
        //bool partially = false, int partiallyCount = 0, long partiallyHistoryTime = 0
        {
            var res = DownloadOrderHistoryInternal(from, to, sort, ascending); //, partially, partiallyCount, partiallyHistoryTime
            if (res == null)
                throw new DoubleRequestException("Download Order History Running");
            else
                return res;
        }

        ConcurrentDictionary<byte, byte> DownloadOrderHistoryRunning = new ConcurrentDictionary<byte, byte>();

        internal OrderHistoryEventArgs DownloadOrderHistoryInternal(DateTime from, DateTime to, OrderSort sort = OrderSort.OpenTime, bool ascending = true,
            DealInternal[] existDeals = null)
        //bool partially = false, int partiallyCount = 0, long partiallyHistoryTime = 0)
        {
            if (!DownloadOrderHistoryRunning.TryAdd(0, 0))
                return null; // throw new Exception("Download Order History Running");
            try
            {
                Log.trace("DownloadOrderHistory in");
                OrderHistoryFrom = from;
                OrderHistoryTo = to;
                OrderHistoryRequestedByMonth = false;
                //OrderHistoryPartially = partially;
                OnOrderHistory += MT5API_OnOrderHistory;
                ClearOrdHist();
                OrderHistoryRequestCount = 1;
                RequestOrderHistoryInternal(from, to, null, existDeals); // partiallyCount, partiallyHistoryTime);
                DateTime start = DateTime.Now;
                while (LastOrdHistResp == new DateTime())
                {
                    if (DateTime.Now.Subtract(start).TotalMilliseconds > DownloadOrderHistoryTimeout)
                        throw new TimeoutException("DownloadOrderHistoryTimeout");
                    //if (!Connected)
                    //    throw new ConnectException("Connection lost");
                    Thread.Sleep(100);
                }
                //var ordHist = new OrderHistoryEventArgs();
                //ordHist.Action = DownloadedOrderHistory.Action;
                //ordHist.Orders = DownloadedOrderHistory.Orders.ToList();
                //ordHist.InternalDeals = DownloadedOrderHistory.InternalDeals.ToList();
                //ordHist.InternalOrders = DownloadedOrderHistory.InternalOrders.ToList();
                //if (HasCloseDealWithoutOpenDeal(ordHist, from, to))
                //{
                //    var fromEx = from.AddDays(-7);
                //    ClearOrdHist();
                //    OrderHistoryRequestCount = 1;
                //    RequestOrderHistoryInternal(fromEx, to, null); //, partiallyCount, partiallyHistoryTime);
                //    while (LastOrdHistResp == new DateTime())
                //        System.Threading.Thread.Sleep(100);
                //}
                //if (HasCloseDealWithoutOpenDeal(ordHist, from, to))
                //{
                //    var fromEx = from.AddMonths(-3);
                //    ClearOrdHist();
                //    OrderHistoryRequestCount = 1;
                //    RequestOrderHistoryInternal(fromEx, to, null); //, partiallyCount, partiallyHistoryTime);
                //    while (LastOrdHistResp == new DateTime())
                //        System.Threading.Thread.Sleep(100);
                //    bool b = HasCloseDealWithoutOpenDeal(ordHist, from, to);
                //}
                OnOrderHistory -= MT5API_OnOrderHistory;
                var ordHist = new OrderHistoryEventArgs();
                ordHist.Action = DownloadedOrderHistory.Action;
                ordHist.Orders = DownloadedOrderHistory.Orders.Values.ToList();
                ordHist.InternalDeals = DownloadedOrderHistory.InternalDeals.Values.ToList();
                ordHist.InternalOrders = DownloadedOrderHistory.InternalOrders.Values.ToList();
                //if (!partially)
                //{
                List<Order> del = new List<Order>();
                foreach (var item in ordHist.Orders)
                {
                    if (item.CloseTime == default)
                        if (item.OpenTime < from || item.OpenTime > to)
                            del.Add(item);

                    if (item.CloseTime != ConvertTo.StartTime && item.CloseTime != default)
                        if (item.CloseTime < from)
                            del.Add(item);
                    if (item.CloseTime != ConvertTo.StartTime && item.CloseTime != default)
                        if (item.CloseTime > to)
                            del.Add(item);
                    if (item.OpenTime == default)
                        del.Add(item);
                }
                foreach (var item in del)
                    ordHist.Orders.Remove(item);
                List<DealInternal> delDeal = new List<DealInternal>();
                foreach (var item in ordHist.InternalDeals)
                    if (ConvertTo.DateTime(item.OpenTime) < from || ConvertTo.DateTime(item.OpenTime) > to)
                        delDeal.Add(item);
                foreach (var item in delDeal)
                    ordHist.InternalDeals.Remove(item);

                List<OrderInternal> delOrder = new List<OrderInternal>();
                foreach (var item in ordHist.InternalOrders)
                    if (ConvertTo.DateTime(item.OpenTime) < from || ConvertTo.DateTime(item.OpenTime) > to)
                        delOrder.Add(item);
                foreach (var item in delOrder)
                    ordHist.InternalOrders.Remove(item);
                //}
                if (sort == OrderSort.CloseTime)
                    if (ascending)
                        ordHist.Orders.Sort((s1, s2) => s1.CloseTime.CompareTo(s2.CloseTime));
                    else
                        ordHist.Orders.Sort((s1, s2) => s2.CloseTime.CompareTo(s1.CloseTime));
                else if (sort == OrderSort.OpenTime)
                    if (ascending)
                        ordHist.Orders.Sort((s1, s2) => s1.OpenTime.CompareTo(s2.OpenTime));
                    else
                        ordHist.Orders.Sort((s1, s2) => s2.OpenTime.CompareTo(s1.OpenTime));
                foreach (var item in ordHist.Orders)
                    item.CloseLots = Math.Round(item.CloseLots, 8);
                return ordHist;

            }
            finally
            {
                DownloadOrderHistoryRunning.TryRemove(0, out _);
                Log.trace("DownloadOrderHistory out");
            }
        }

        /// <summary>
        /// Downaload order history without DoubleRequestException. If DoubleRequestException happens function waits until previous request will be finished.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <param name="sort">Sort by open time or close time</param>
        /// <param name="ascending">Ascending sort</param>
        /// <returns>Array of orders.</returns>
        public Task<List<OrderInternal>> DownloadPendingOrderHistoryAsync(DateTime from, DateTime to, bool ascending = true)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var res = DownloadPendingOrderHistoryInternal(from, to, ascending);
                    if (res != null)
                        return res;
                    await Task.Delay(100);
                }
            });
        }

        /// <summary>
        /// Downaload order history.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <param name="from">Start time of history.</param>
        /// <param name="to">End time of history.</param>
        /// <param name="sort">Sort by open time or close time</param>
        /// <param name="ascending">Ascending sort</param>
        /// <returns>Array of orders.</returns>
        public List<OrderInternal> DownloadPendingOrderHistory(DateTime from, DateTime to, bool ascending = true)
        {
            var res = DownloadPendingOrderHistoryInternal(from, to, ascending);
            if (res == null)
                throw new DoubleRequestException("Download Order History Running");
            else
                return res;
        }


        internal List<OrderInternal> DownloadPendingOrderHistoryInternal(DateTime from, DateTime to, bool ascending = true)
        {
            if (!DownloadOrderHistoryRunning.TryAdd(0, 0))
                return null; // throw new Exception("Download Order History Running");
            try
            {
                Log.trace("DownloadOrderHistory");
                OrderHistoryTo = to;
                OnOrderHistory += MT5API_OnOrderHistory;
                ClearOrdHist();
                OrderHistoryRequestCount = 1;
                RequestPendingOrderHistoryInternal(from, to);
                DateTime start = DateTime.Now;
                while (LastOrdHistResp == new DateTime())
                {
                    if (DateTime.Now.Subtract(start).TotalMilliseconds > DownloadOrderHistoryTimeout)
                        throw new TimeoutException("DownloadOrderHistoryTimeout");
                    //if (!Connected)
                    //    throw new ConnectException("Connection lost");
                    Thread.Sleep(100);
                }
                OnOrderHistory -= MT5API_OnOrderHistory;
                var ordHist = new OrderHistoryEventArgs();
                ordHist.Action = DownloadedOrderHistory.Action;
                ordHist.Orders = DownloadedOrderHistory.Orders.Values.ToList();
                ordHist.InternalDeals = DownloadedOrderHistory.InternalDeals.Values.ToList();
                ordHist.InternalOrders = DownloadedOrderHistory.InternalOrders.Values.ToList();

                List<OrderInternal> delOrder = new List<OrderInternal>();
                foreach (var item in ordHist.InternalOrders)
                    if (ConvertTo.DateTime(item.OpenTime) < from || ConvertTo.DateTime(item.OpenTime) > to)
                        delOrder.Add(item);
                foreach (var item in delOrder)
                    ordHist.InternalOrders.Remove(item);
                if (ascending)
                    ordHist.InternalOrders.Sort((s1, s2) => s1.OpenTime.CompareTo(s2.OpenTime));
                else
                    ordHist.InternalOrders.Sort((s1, s2) => s2.OpenTime.CompareTo(s1.OpenTime));
                return ordHist.InternalOrders;
            }
            finally
            {
                DownloadOrderHistoryRunning.TryRemove(0, out _);
            }
        }


        void ClearOrdHist()
        {
            LastOrdHistResp = new DateTime();
            DownloadedOrderHistory = new OrderHistoryEventArgsInternal();
            DownloadedOrderHistory.Orders = new ConcurrentDictionary<long, Order>();
            DownloadedOrderHistory.InternalDeals = new ConcurrentDictionary<long, DealInternal>();
            DownloadedOrderHistory.InternalOrders = new ConcurrentDictionary<long, OrderInternal>();
        }

        bool HasCloseDealWithoutOpenDeal(OrderHistoryEventArgs OrdHist, DateTime from, DateTime to)
        {
            foreach (var item in OrdHist.Orders)
            {
                if (item.DealInternalIn == null)
                {
                    if (item.DealInternalOut != null)
                    {
                        var deal = item.DealInternalOut;
                        if (deal.Type == DealType.DealBuy || deal.Type == DealType.DealSell)
                        {
                            if (deal.Direction == Direction.Out
                                || deal.Direction == Direction.OutBy)
                                if (deal.OpenTimeAsDateTime > from && deal.OpenTimeAsDateTime < to)
                                    return true;
                        }
                    }
                }
            }
            return false;
        }

        private void MT5API_OnOrderHistory(MT5API sender, OrderHistoryEventArgs args)
        {
            if (!DownloadOrderHistoryRunning.ContainsKey(0))
                return;
            DownloadedOrderHistory.Action = args.Action;
            if (args.Orders != null)
                foreach (var item in args.Orders)
                    DownloadedOrderHistory.Orders.TryAdd(item.Ticket, item);

            if (args.InternalDeals != null)
                foreach (var deal in args.InternalDeals)
                    if (deal.TicketNumber != 0)
                        DownloadedOrderHistory.InternalDeals.TryAdd(deal.TicketNumber, deal);
                    else
                        DownloadedOrderHistory.InternalDeals.TryAdd(deal.OrderTicket, deal);

            if (args.InternalOrders != null)
                foreach (var order in args.InternalOrders)
                    DownloadedOrderHistory.InternalOrders.TryAdd(order.Ticket, order);

            //Console.WriteLine("Action = " + args.Action);
            //Console.WriteLine("Partial = " + args.PartialResponse);
            //Console.WriteLine("orders = " + args.Orders.Count);
            //Console.WriteLine("first = " + args.Orders.First().CloseTime);
            //Console.WriteLine("last = " + args.Orders.Last().CloseTime);
            //Console.WriteLine("internal_deals = " + args.InternalDeals?.Count);
            //Console.WriteLine("internal_deals = " + args.InternalOrders?.Count);
            if (args.PartialResponse)// && !OrderHistoryPartially)
            {

                if (args.InternalDeals != null)
                    if (args.InternalDeals.Count > 0)
                    {
                        DealInternal last = args.InternalDeals[args.InternalDeals.Count - 1];
                        DateTime start = new DateTime(last.OpenTimeAsDateTime.Year, last.OpenTimeAsDateTime.Month, 1);
                        //Console.WriteLine("Requesting more deals for " + start.Month);
                        OrderHistoryRequestCount++;
                        RequestDealHistory(start.Year, start.Month, args.InternalDeals);
                        if (!OrderHistoryRequestedByMonth)
                        {
                            OrderHistoryRequestedByMonth = true;
                            while (start < DateTime.Now)
                            {
                                start = start.AddMonths(1);
                                if (start < ServerTime)
                                {
                                    //Console.WriteLine("Requesting more deals monthly " + last.OpenTimeAsDateTime + " to " + DateTime.Now);
                                    OrderHistoryRequestCount++;
                                    RequestDealHistory(start.Year, start.Month, args.InternalDeals);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                    }
                if (args.InternalOrders != null)
                    if ((args.InternalOrders.Count > 0))
                    {
                        OrderInternal last = args.InternalOrders[args.InternalOrders.Count - 1];
                        DateTime start = new DateTime(last.OpenTimeAsDateTime.Year, last.OpenTimeAsDateTime.Month, 1);
                        //Console.WriteLine("Requesting more orders for " + start.Month);
                        OrderHistoryRequestCount++;
                        RequestPendingHistory(start.Year, start.Month, args.InternalOrders);
                        if (!OrderHistoryRequestedByMonth)
                        {
                            OrderHistoryRequestedByMonth = true;
                            while (start < DateTime.Now)
                            {
                                start = start.AddMonths(1);
                                if (start < ServerTime)
                                {
                                    //Console.WriteLine("Requesting more deals monthly " + last.OpenTimeAsDateTime + " to " + DateTime.Now);
                                    OrderHistoryRequestCount++;
                                    RequestPendingHistory(start.Year, start.Month, args.InternalOrders);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                    }
            }
            OrderHistoryRequestCount--;
            //Console.WriteLine("OrderHistoryRequestCount = " + OrderHistoryRequestCount);
            if (OrderHistoryRequestCount == 0)
                LastOrdHistResp = DateTime.Now;
        }

        /// <summary>
        /// Milliseconds. Throw ConnectException if not connected at this period.
        /// </summary>
        public int ConnectTimeout = 30000;

        /// <summary>
        /// Milliseconds. Conbnect timeout for one cluster member.
        /// </summary>
        public int ConnectTimeoutForOneClusterMember = 20000;

        /// <summary>
        /// Enable order updates events Cancelling, Started, Filled, Unknown.
        /// </summary>
        public bool EnableAllOrderUpdates { get; set; } = false;

        /// <summary>
        /// Connect to server thread.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        private void ConnectInThread(int connectTimeout)
        {
            Exception ex = null;
            // host == null if server name was used in MT5API constructor
            var totalTimeout = connectTimeout;
            var start = DateTime.Now;
            if (Host != null)
                try
                {
                    int timeout = connectTimeout;
                    if (!ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>())) //more than 1 server
                        if (timeout > ConnectTimeoutForOneClusterMember)
                            timeout = ConnectTimeoutForOneClusterMember;
                    Connector.run(timeout, Host, Port, totalTimeout);
                    return;
                }
                catch (ServerException e)
                {
                    ex = e;
                    if (e.Code == Msg.INVALID_ACCOUNT)
                        throw;
                    
                }
                catch (Exception e)
                {
                    ex = e;
                }
            connectTimeout -= (int)DateTime.Now.Subtract(start).TotalMilliseconds;
            // ServerDetails is empty if host and port was used in MT5API constructor
            if(connectTimeout > 0)
                if (!ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
                {
                    var serverList = ServerDetails.Value.ToList();
                    var random = new Random();
                    var shuffledServers = serverList.OrderBy(_ => random.Next()).ToList();

                    foreach (var item in shuffledServers)
                    {
                        start = DateTime.Now;
                        try
                        {
                            var hostport = HostAndPort.Parse(item.Value[0].Address);
                            if(IsLocalNetwork(hostport.Key))
                                continue;
                            int timeout = connectTimeout;
                            if (timeout > ConnectTimeoutForOneClusterMember)
                                timeout = ConnectTimeoutForOneClusterMember;
                            Connector.run(timeout, hostport.Key, hostport.Value, totalTimeout);
                            return;
                        }
                        catch (ServerException e)
                        {
                            if (e.Code == Msg.INVALID_ACCOUNT)
                                throw;
                            ex = e;
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }
                        connectTimeout -= (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                        if (connectTimeout <= 0)
                            break;
                    }
                }
            if (!Connected)
                if (ex != null)
                    throw ex;
                else
                    throw new ConnectException("ServerDetails and host not specified or connectTimeout too small");
        }

        bool IsLocalNetwork(string host)
        {
            try
            {
                IPAddress[] addresses;

                // Try parsing as IP address first
                if (IPAddress.TryParse(host, out IPAddress ip))
                {
                    addresses = new[] { ip };
                }
                else
                {
                    // Resolve DNS name to IP addresses
                    addresses = Dns.GetHostAddresses(host);
                }

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        byte[] bytes = address.GetAddressBytes();

                        if (bytes[0] == 10 ||
                            (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                            (bytes[0] == 192 && bytes[1] == 168))
                        {
                            return true;
                        }
                    }

                    // Optionally handle IPv6 local addresses
                    if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // DNS resolution failed or other error
            }

            return false;
        }

        /// <summary>
        /// Connect to server thread.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        /// <summary>
        /// Connect to server thread.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        private async Task ConnectInTask(CancellationToken cancellation)
        {
            Exception ex = null;

            // host == null if server name was used in MT5API constructor
            if (Host != null)
            {
                try
                {
                    await ConnectorTask.run(Host, Port, cancellation);
                    return;
                }
                catch (ServerException e)
                {
                    ex = e;
                    if (e.Code == Msg.INVALID_ACCOUNT)
                        throw;
                    if (ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
                        throw;
                }
                catch (Exception e)
                {
                    ex = e;
                    if (ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
                        throw;
                }
            }

            // If host/port not given, fall back to server list
            if (!ServerDetails.Equals(new KeyValuePair<ServerRec, KeyValuePair<AccessInfo, AddressRec[]>[]>()))
            {
                var serverList = ServerDetails.Value.ToList();
                var random = new Random();
                var shuffledServers = serverList.OrderBy(_ => random.Next()).ToList();

                foreach (var item in shuffledServers)
                {
                    if (item.Value == null || item.Value.Length == 0) continue;

                    try
                    {
                        var hostport = HostAndPort.Parse(item.Value[0].Address);
                        if (IsLocalNetwork(hostport.Key))
                            continue;

                        using (var perServerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                        {
                            perServerCts.CancelAfter(ConnectTimeoutForOneClusterMember);
                            await ConnectorTask.run(hostport.Key, hostport.Value, perServerCts.Token);
                        }
                        return;
                    }
                    catch (ServerException e)
                    {
                        ex = e;
                        if (e.Code == Msg.INVALID_ACCOUNT)
                            throw;
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                    if (cancellation.IsCancellationRequested)
                        break;
                }
            }

            // Still not connected
            if (!Connected)
            {
                if (ex != null)
                    throw ex;
                throw new ConnectException("ServerDetails and host not specified");
            }
        }

        /// <summary>
        /// Connect to server.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        public virtual void Connect()
        {
            if (UseConnectTask)
                try
                {
                    var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(ConnectTimeout)).Token;
                    ConnectInTask(token).Wait();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    else
                        throw;
                }
            else
                ConnectInThread(ConnectTimeout);
        }

        /// <summary>
        /// Connect to server.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        public async Task ConnectAsync(CancellationToken cancellation)
        {
            UseConnectTask = true;
            if(cancellation == null)
                cancellation = new CancellationTokenSource(ConnectTimeout).Token;
            await ConnectInTask(cancellation);
        }

        /// <summary>
        /// Connect to server.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        public async Task ConnectAsync(int timeoutMs)
        {
            UseConnectTask = true;
            var cancellation = new CancellationTokenSource(timeoutMs).Token;
            await ConnectInTask(cancellation);
        }

        /// <summary>
        /// Connect to server.
        /// </summary>
        /// <exception cref="ServerException">Unable to login for some reason.</exception>
        /// <exception cref="TimeoutException">No reply from server.</exception>
        public async Task ConnectAsync()
        {
            UseConnectTask = true;
            var cancellation = new CancellationTokenSource(ConnectTimeout).Token;
            await ConnectInTask(cancellation);
        }

        /// <summary>
        /// Tick size
        /// </summary>
        public double GetTickSize(string symbol)
        {
            var i = Symbols.GetInfo(symbol);
            if (i.TickSize == 0)
                return i.Points;
            else
                return i.TickSize;
        }

        /// <summary>
        /// Tick value
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="timeout">Timeout in ms for waiting quote</param>
        /// <returns></returns>
        public double GetTickValue(string symbol, int msGetQuoteTimeout = 10000)
        {
            var i = Symbols.GetInfo(symbol);
            if (i.TickValue != 0)
                return i.TickValue;
            new OrderProfit(this).UpdateSymbolTick(i, msGetQuoteTimeout).Wait();
            return (i.ask_tickvalue + i.bid_tickvalue) / 2;
        }

        /// <summary>
        /// Tick value
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="timeout">Timeout in ms for waiting quote</param>
        /// <returns></returns>
        public async Task<double> GetTickValueAsync(string symbol, int msGetQuoteTimeout = 10000)
        {
            var i = Symbols.GetInfo(symbol);
            if (i.TickValue != 0)
                return i.TickValue;
            await new OrderProfit(this).UpdateSymbolTick(i, msGetQuoteTimeout);
            return (i.ask_tickvalue + i.bid_tickvalue) / 2;
        }


        /// <summary>
        /// Tick value in base currency
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public double GetBidTickValue(Quote quote)
        {
            var i = Symbols.GetInfo(quote.Symbol);
            new OrderProfit(this).UpdateSymbolTick(i, GetQuoteTimeoutMs).Wait();
            return i.bid_tickvalue;
        }



        /// <summary>
        /// Tick value in base currency
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<double> GetBidTickValueAsync(Quote quote)
        {
            var i = Symbols.GetInfo(quote.Symbol);
            await new OrderProfit(this).UpdateSymbolTick(i, GetQuoteTimeoutMs);
            return i.bid_tickvalue;
        }

        /// <summary>
        /// Tick value in base currency
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public double GetAskTickValue(Quote quote)
        {
            var i = Symbols.GetInfo(quote.Symbol);
            new OrderProfit(this).UpdateSymbolTick(i, GetQuoteTimeoutMs).Wait();
            return i.ask_tickvalue;
        }

        // <summary>
        /// Tick value in base currency
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<double> GetAskTickValueAsync(Quote quote)
        {
            var i = Symbols.GetInfo(quote.Symbol);
            await new OrderProfit(this).UpdateSymbolTick(i, GetQuoteTimeoutMs);
            return i.ask_tickvalue;
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            Symbols.RemoveInstance(ClusterSummary.ServerName, Symbols);
            if (CmdHandler != null)
                CmdHandler.StopCmdHandler();
            CmdHandler.Thread.Join(1000);
            Symbols.Base = null;
            Symbols.Groups = null;
            Symbols.Sessions = null;
            Symbols.SymGroups = null;
            Symbols.Infos = null;
            Symbols.InfosById = null;
        }


        internal void OnConnectCall(Exception exception, ConnectProgress progress)
        {
            ConnectEventArgs args = new ConnectEventArgs
            {
                Exception = exception,
                Progress = progress
            };
            ThreadPool.QueueUserWorkItem(OnConnectThread, args, ProcessEventTimeoutMs);
        }

        internal void OnMailCall(MailMessage msg)
        {
            ThreadPool.QueueUserWorkItem(onMail, msg, ProcessEventTimeoutMs);
        }

        private void onMail(object args)
        {
            try
            {
                if (OnMail != null && Connected)
                    OnMail(this, (MailMessage)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        //public int DisconnectTimeout { get; internal set; }

        internal void OnDisconnect(Exception ex)
        {
            if (ex != null)
                Log.exception(ex, this);
            else
                Log.trace("onDisconnect");
            OnConnectCall(ex, ConnectProgress.Disconnect);
        }

        internal void OnQuoteHistoryCall(string symbol, List<Bar> bars)
        {
            QuoteHistoryEventArgs args = new QuoteHistoryEventArgs
            {
                Symbol = symbol,
                Bars = bars
            };
            ThreadPool.QueueUserWorkItem(onQuoteHist, args, ProcessEventTimeoutMs);
        }

        private void onQuoteHist(object args)
        {
            try
            {
                DownloadQuoteHistoryUpdate(this, (QuoteHistoryEventArgs)args);
                if (OnQuoteHistory != null)
                    OnQuoteHistory(this, (QuoteHistoryEventArgs)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        internal void OnTickHistoryCall(string symbol, TickBar[] bars)
        {
            var args = new TickHistoryEventArgs
            {
                Symbol = symbol,
                Bars = bars
            };
            ThreadPool.QueueUserWorkItem(onTickHist, args, ProcessEventTimeoutMs);
        }

        private void onTickHist(object args)
        {
            try
            {
                if (OnTickHistory != null)
                    OnTickHistory(this, (TickHistoryEventArgs)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        internal void OnOrderBookCall(SymbolBook rec)
        {
            ThreadPool.QueueUserWorkItem(onOrderBook, rec, ProcessEventTimeoutMs);
        }

        private void onOrderBook(object args)
        {
            try
            {
                if (OnOrderBook != null)
                    OnOrderBook(this, (SymbolBook)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }


        /// <summary>
        /// Request 1 minute bar hsitory for one month back from specifeid date
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        public void RequestQuoteHistoryMonth(string symbol, int year, int month, int day)
        {
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            QuoteHistory.ReqSend(Connection, symbol, Date.Convert(4, 1, 1), Date.Convert(day, month, year - 1970)).Wait();
        }



        /// <summary>
        /// Request 1 minute bar hsitory for today
        /// </summary>
        /// <param name="symbol">Symbol</param>
        public void RequestQuoteHistoryToday(string symbol)
        {
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            QuoteHistory.ReqStart(Connection, symbol, 33).Wait();
        }

        /// <summary>
        /// Subscribe trading instrument.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void Subscribe(string symbol)
        {
            Log.trace("Subscribe");
            if (!Connected)
                Connect(); //throw new Exception("Not connected");
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            Subscriber.Subscribe(symbol).Wait();
        }


        /// <summary>
        /// Subscribe order book.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void SubscribeOrderBook(string symbol)
        {
            Log.trace("SubscribeOrderBook");
            if (!Connected)
                Connect(); //throw new Exception("Not connected");
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            OrderBook.Subscribe(symbol);
        }

        /// <summary>
        /// Subscribe order book.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void UnsubscribeOrderBook(string symbol)
        {
            Log.trace("SubscribeOrderBook");
            if (!Connected)
                Connect(); //throw new Exception("Not connected");
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            OrderBook.Unsubscribe(symbol);
        }

        /// <summary>
        /// Subscribe trading instruments.
        /// </summary>
        /// <param name="symbols">Symbols for trading.</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void Subscribe(string[] symbols)
        {
            Log.trace("Subscribe many");
            if (!Connected)
                Connect();
            foreach (var symbol in symbols)
                if (!Symbols.Exist(symbol))
                    throw new InvalidSymbolException(symbol + " not exist");
            Subscriber.Subscribe(symbols).Wait();
        }

        /// <summary>
        /// Experimental. Do request to mt5 server on each call.
        /// </summary>
        /// <param name="symbols">Symbols for trading.</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void SubscribeForce(string[] symbols)
        {
            Log.trace("Subscribe force");
            if (!Connected)
                Connect();
            foreach (var symbol in symbols)
                if (!Symbols.Exist(symbol))
                    throw new InvalidSymbolException(symbol + " not exist");
            Subscriber.SubscribeForce(symbols).Wait();
        }


        /// <summary>
        /// Unsubscribe trading instrument.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void Unsubscribe(string symbol)
        {
            Log.trace("Unsubscribe");
            if (!Connected)
                throw new Exception("Not connected");
            Subscriber.Unsubscribe(symbol).Wait();
        }

        /// <summary>
        /// Unsubscribe trading instruments.
        /// </summary>
        /// <param name="symbols">Symbols</param>
        /// <exception cref="Exception">Not connected.</exception>
        public void Unsubscribe(string[] symbols)
        {
            Log.trace("Unsubscribe");
            if (!Connected)
                throw new Exception("Not connected");
            Subscriber.Unsubscribe(symbols).Wait();
        }

        /// <summary>
        /// List of subscribed symbols
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string[] Subscriptions()
        {
            Log.trace("Subscriptions");
            if (!Connected)
                throw new Exception("Not connected");
            return Subscriber.Subscriptions();
        }

        ConcurrentDictionary<string, AsyncBroadcastEvent> GetQuoteEvents = new ConcurrentDictionary<string, AsyncBroadcastEvent>();

        /// <summary>
        /// Latest quote for the symbol.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <param name="msNotOlder">If last quote arrived less than msOlder milliseconds ago function returns last quote, overwise - wait for new quote</param>
        /// <exception cref="InvalidSymbolException">Symbol not exist.</exception>
        /// <exception cref="TimeoutException">Timeout.</exception>
        /// <returns>Return null if no quotes for specified symbol avalible and msTimeout equals to zero, otherwise return quote event arguments or throws exception if first quote not arrived during msTimeout milliseconds</returns>
        public async Task<Quote> GetQuoteAsync(string symbol, int msTimeout = 5000, int msNotOlder = 0)
        {
            DateTime start = DateTime.Now;
            while (true)
            {
                var quote = await Subscriber.GetQuote(symbol, msNotOlder);
                if (quote != null)
                    if (quote.Bid != 0 && quote.Ask != 0 &&
                        !double.IsNaN(quote.Bid) && !double.IsNaN(quote.Ask) &&
                        !double.IsInfinity(quote.Bid) && !double.IsInfinity(quote.Ask))
                            return quote;
                if (DateTime.Now.Subtract(start).TotalMilliseconds >= msTimeout)
                    throw new TimeoutException("Cannot get quote in " + msTimeout + " ms");
                GetQuoteEvents.TryAdd(symbol, new AsyncBroadcastEvent());
                if (GetQuoteEvents.TryGetValue(symbol, out var evnt))
                    await Task.WhenAny(evnt.WaitAsync(), Task.Delay(500));
            }
        }

        /// <summary>
        /// Latest quote for the symbol.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <param name="msNotOlder">If last quote arrived less than msOlder milliseconds ago function returns last quote, overwise - wait for new quote</param>
        /// <exception cref="InvalidSymbolException">Symbol not exist.</exception>
        /// <exception cref="TimeoutException">Timeout.</exception>
        /// <returns>Return null if no quotes for specified symbol avalible and msTimeout equals to zero, otherwise return quote event arguments or throws exception if first quote not arrived during msTimeout milliseconds</returns>
        public async Task<Quote> GetQuoteTask(string symbol, int msTimeout = 5000, int msNotOlder = 0)
        {
            return await GetQuoteAsync(symbol, msTimeout, msNotOlder);
        }


        /// <summary>
        /// Latest quote for the symbol.
        /// </summary>
        /// <param name="symbol">Symbol for trading.</param>
        /// <param name="msNotOlder">If last quote arrived less than msOlder milliseconds ago function returns last quote, overwise - wait for new quote</param>
        /// <exception cref="InvalidSymbolException">Symbol not exist.</exception>
        /// <exception cref="TimeoutException">Timeout.</exception>
        /// <returns>Return null if no quotes for specified symbol avalible and msTimeout equals to zero, otherwise return quote event arguments or throws exception if first quote not arrived during msTimeout milliseconds</returns>
        public Quote GetQuote(string symbol, int msTimeout = 0, int msNotOlder = 0)
        {
            // wait for connection establishing before getting quotes if GetQuote called internally during connection establishing
            if (!Connected)
                Connect();
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException($"Symbol {symbol} not exist");
            DateTime start = DateTime.Now;
            var subscribeForceCalled = false;
            while (true)
            {
                var quote = Subscriber.GetQuote(symbol, msNotOlder).Result;
                if (quote == null)
                    if (msTimeout == 0)
                        return null; // old behaviuor
                if (quote != null)
                    if (quote.Bid != 0 && quote.Ask != 0 &&
                        !double.IsNaN(quote.Bid) && !double.IsNaN(quote.Ask) &&
                        !double.IsInfinity(quote.Bid) && !double.IsInfinity(quote.Ask))
                    {
                        return quote;
                    }
                if (DateTime.Now.Subtract(start).TotalMilliseconds > msTimeout)
                    throw new TimeoutException($"Cannot get quote for {symbol} in {msTimeout} ms. SubscribeForceCalled = " + subscribeForceCalled + ", msNotOlder = " + msNotOlder);
                if (DateTime.Now.Subtract(start).TotalMilliseconds >= (msTimeout / 2) && !subscribeForceCalled)
                {
                    Subscriber.Quotes.TryRemove(symbol, out _);
                    Subscriber.SubscribeForce(Subscriptions()).Wait();
                    Thread.Sleep(100);
                    Subscriber.Subscribe(symbol).Wait();
                    subscribeForceCalled = true;
                }
                GetQuoteEvents.TryAdd(symbol, new AsyncBroadcastEvent());
                if (GetQuoteEvents.TryGetValue(symbol, out var evnt))
                    evnt.WaitAsync().Wait(500);
            }
        }

        public Quote GetQuoteFromAnyServer(string symbol)
        {
            return Subscriber.GetQuoteFromAnyServer(symbol);
        }


        internal async Task<Quote> GetQuoteInternal(string symbol, int msTimeout)
        {
            if (!Symbols.Exist(symbol))
                return null;
            DateTime start = DateTime.Now;
            while (true)
            {
                var quote = await Subscriber.GetQuoteInternal(symbol);
                if (quote != null)
                    if (quote.Bid != 0 && quote.Ask != 0)
                        return quote;
                if (DateTime.Now.Subtract(start).TotalMilliseconds > msTimeout)
                    return null;
                GetQuoteEvents.TryAdd(symbol, new AsyncBroadcastEvent());
                if (GetQuoteEvents.TryGetValue(symbol, out var evnt))
                    await Task.WhenAny(evnt.WaitAsync(), Task.Delay(500));
            }
        }

        /// <summary>
        /// Market watch for the symbol.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns></returns>
        public MarketWatch GetMarketWatch(string symbol)
        {
            GetQuote(symbol, GetQuoteTimeoutMs);
            return Subscriber.GetMarketWatch(symbol).Result;
        }

        /// <summary>
        /// Market watch for the symbol.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns></returns>
        public async Task<MarketWatch> GetMarketWatchAsync(string symbol)
        {
            await GetQuoteAsync(symbol, GetQuoteTimeoutMs);
            return await Subscriber.GetMarketWatch(symbol);
        }

        public List<Bar> ConvertToTimeframe(List<Bar> bars, int minutes)
        {
            if (minutes == 1)
                return bars;
            if (minutes > 60)
                if (minutes % 60 != 0)
                    throw new Exception("If timeframe > 60 it should be in whole hours");
            if (bars.Count == 0)
                return new List<Bar>();
            int i = 0;
            if (minutes <= 60)
                while (bars[i].Time.Minute % minutes > 0)
                    if (i >= bars.Count)
                        return new List<Bar>();
                    else
                        i++;
            List<Bar> res = new List<Bar>();
            Bar bar = new Bar();
            bar.OpenPrice = bars[0].OpenPrice;
            bar.HighPrice = bars[0].HighPrice;
            bar.LowPrice = bars[0].LowPrice;
            bar.ClosePrice = bars[0].ClosePrice;
            bar.Time = bars[0].Time;
            bar.Volume = bars[0].Volume;
            bar.TickVolume = bars[0].TickVolume;
            DateTime time = bars[0].Time.AddMinutes(minutes);
            time = time.AddMinutes(-time.Minute % minutes);
            if (minutes == 1440)
                time = new DateTime(time.Year, time.Month, time.Day);
            if (minutes == 1440 * 7)
                time = new DateTime(time.Year, time.Month, time.Day).AddDays(-(int)time.DayOfWeek);
            if (minutes == 1440 * 31)
                time = new DateTime(time.Year, time.Month, 1);
            for (; i < bars.Count; i++)
            {
                if (i == bars.Count - 1 && bars[i].Time < time)
                {
                    bar.ClosePrice = bars[i].ClosePrice;
                    res.Add(bar);
                }
                else if (bars[i].Time >= time)
                {
                    bar.ClosePrice = bars[i - 1].ClosePrice;
                    //if (minutes == 1440 * 31)
                    //    bar.Time = new DateTime(bar.Time.Year, bar.Time.Month, 1);
                    res.Add(bar);
                    bar = new Bar();
                    bar.OpenPrice = bars[i].OpenPrice;
                    bar.HighPrice = bars[i].HighPrice;
                    bar.LowPrice = bars[i].LowPrice;
                    bar.ClosePrice = bars[i].ClosePrice;
                    bar.Time = bars[i].Time;
                    while (time <= bar.Time)
                        time = time.AddMinutes(minutes);
                    if (minutes == 1440)
                    {
                        var nextday = bars[i].Time.AddHours(24);
                        time = new DateTime(nextday.Year, nextday.Month, nextday.Day);
                    }
                    else if (minutes == 1440 * 7)
                        time = new DateTime(time.Year, time.Month, time.Day).AddDays(-(int)time.DayOfWeek);
                    else if (minutes == 1440 * 31)
                        time = new DateTime(time.Year, time.Month, 1);
                }
                if (bars[i].HighPrice > bar.HighPrice)
                    bar.HighPrice = bars[i].HighPrice;
                if (bars[i].LowPrice < bar.LowPrice)
                    bar.LowPrice = bars[i].LowPrice;
                bar.Volume += bars[i].Volume;
                bar.TickVolume += bars[i].TickVolume;
            }
            bar.ClosePrice = bars[i - 1].ClosePrice;
            res.Add(bar);
            foreach (var item in res)
            {
                if (item.Volume == 0)
                    item.Volume = item.TickVolume;
                if (item.HighPrice == 0)
                    Math.Max(item.ClosePrice, Math.Max(item.OpenPrice, item.LowPrice));
                if (item.ClosePrice == 0)
                    Math.Max(item.OpenPrice, Math.Max(item.HighPrice, item.LowPrice));
                if (item.OpenPrice == 0)
                    Math.Max(item.ClosePrice, Math.Max(item.HighPrice, item.LowPrice));
                if (item.LowPrice == 0)
                    Math.Min(item.ClosePrice, Math.Max(item.OpenPrice, item.HighPrice));
            }
            return res;
        }

        private void RequestQuoteHistoryMonthInternal(string symbol, int year, int month, int day)
        {
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            QuoteHistory.ReqSend(Connection, symbol, Date.Convert(4, 1, 1), Date.Convert(day, month, year - 1970)).Wait();
        }

        public void RequestQuoteHistoryTodayInternal(string symbol)
        {
            if (!Symbols.Exist(symbol))
                throw new InvalidSymbolException(symbol + " not exist"); ;
            QuoteHistory.ReqStart(Connection, symbol, 33).Wait();
        }


        class HistoryRequest
        {
            public ManualResetEvent Event = new ManualResetEvent(false);
            public List<Bar> Bars = null;
        }

        ConcurrentDictionary<string, HistoryRequest> ActiveRequests = new ConcurrentDictionary<string, HistoryRequest>();
        ConcurrentDictionary<string, ManualResetEvent> DownloadQuoteHistoryLock = new ConcurrentDictionary<string, ManualResetEvent>();

        /// <summary>
        /// Downlowd quote history without DoubleRequestException. If DoubleRequestException happens function waits until previous request will be finished.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="timeFrame">Timeframe in minutes</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns></returns>
        /// <exception cref="DoubleRequestException"></exception>
        /// <exception cref="InvalidSymbolException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public Task<List<Bar>> DownloadQuoteHistoryMonthAsync(string symbol, int year, int month, int day, int timeFrame,
            int timeoutMs = 15000)
        {
            return Task<OrderHistoryEventArgs>.Run(async () =>
            {
                DateTime start = DateTime.Now;
                while (true)
                    try
                    {
                        return DownloadQuoteHistoryMonth(symbol, year, month, day, timeFrame, timeoutMs);
                    }
                    catch (DoubleRequestException)
                    {
                        if (DateTime.Now.Subtract(start).TotalMilliseconds > timeoutMs)
                            throw new TimeoutException("Cannot DownloadQuoteHistory in " + timeoutMs + " ms");
                        await Task.Delay(100);
                    }
            });
        }

        /// <summary>
        /// Downlowd quote history for specified symbol and time frame for one month.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="timeFrame">Timeframe in minutes</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns></returns>
        /// <exception cref="DoubleRequestException"></exception>
        /// <exception cref="InvalidSymbolException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public List<Bar> DownloadQuoteHistoryMonth(string symbol, int year, int month, int day, int timeFrame,
            int timeoutMs = 15000)
        {
            if (!DownloadQuoteHistoryLock.TryAdd(symbol, new ManualResetEvent(false)))
                throw new DoubleRequestException($"Another quote history request for {symbol} still running");
            try
            {
                Log.trace("DownloadQuoteHistoryMonth");
                if (!Symbols.Exist(symbol))
                    throw new InvalidSymbolException(symbol + " not exist"); ;
                if (!Connected)
                    Connect();
                var req = new HistoryRequest();
                if (!ActiveRequests.TryAdd(symbol, req))
                    throw new DoubleRequestException($"Previuos request still running for {symbol}");
                try
                {
                    RequestQuoteHistoryMonthInternal(symbol, year, month, day);
                    if (!req.Event.WaitOne(timeoutMs))
                        throw new TimeoutException("Cannot DownloadQuoteHistory in " + timeoutMs + " ms");
                    return ConvertToTimeframe(req.Bars, timeFrame);
                }
                finally
                {
                    ActiveRequests.TryRemove(symbol, out _);
                }
            }
            finally
            {
                if (DownloadQuoteHistoryLock.TryRemove(symbol, out var evnt))
                    evnt.Set();
            }
        }

        /// <summary>
        /// Downlowd quote history without DoubleRequestException. If DoubleRequestException happens function waits until previous request will be finished.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="timeFrame">Timeframe in minutes</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns></returns>
        /// <exception cref="DoubleRequestException"></exception>
        /// <exception cref="InvalidSymbolException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public Task<List<Bar>> DownloadQuoteHistoryTodayAsync(string symbol, int timeFrame, int timeoutMs = 15000)
        {
            return Task<OrderHistoryEventArgs>.Run(async () =>
            {
                DateTime start = DateTime.Now;
                while (true)
                    try
                    {
                        return DownloadQuoteHistoryToday(symbol, timeFrame, timeoutMs);
                    }
                    catch (DoubleRequestException)
                    {
                        if (DateTime.Now.Subtract(start).TotalMilliseconds > timeoutMs)
                            throw new TimeoutException("Cannot DownloadQuoteHistory in " + timeoutMs + " ms");
                        await Task.Delay(100);
                    }
            });
        }


        /// <summary>
        /// Downlowd quote history for specified symbol and time frame for one month.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="timeFrame">Timeframe in minutes</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns></returns>
        /// <exception cref="DoubleRequestException"></exception>
        /// <exception cref="InvalidSymbolException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public List<Bar> DownloadQuoteHistoryToday(string symbol, int timeFrame, int timeoutMs = 15000)
        {
            if (!DownloadQuoteHistoryLock.TryAdd(symbol, new ManualResetEvent(false)))
                throw new DoubleRequestException($"Another quote history request for {symbol} still running");
            try
            {
                Log.trace("DownloadQuoteHistoryToday");
                if (!Symbols.Exist(symbol))
                    throw new InvalidSymbolException(symbol + " not exist"); ;
                if (!Connected)
                    Connect();
                var req = new HistoryRequest();
                if (!ActiveRequests.TryAdd(symbol, req))
                    throw new DoubleRequestException("Previuos request still running");
                try
                {
                    RequestQuoteHistoryTodayInternal(symbol);
                    if (!req.Event.WaitOne(timeoutMs))
                        throw new TimeoutException("Cannot DownloadQuoteHistory in " + timeoutMs + " ms");
                    return ConvertToTimeframe(req.Bars, timeFrame);
                }
                finally
                {
                    ActiveRequests.TryRemove(symbol, out _);
                }
            }
            finally
            {
                if (DownloadQuoteHistoryLock.TryRemove(symbol, out var evnt))
                    evnt.Set();
            }
        }

        void DownloadQuoteHistoryUpdate(MT5API sender, QuoteHistoryEventArgs progress)
        {
            if (ActiveRequests.TryGetValue(progress.Symbol, out var req))
            {
                req.Bars = progress.Bars;
                req.Event.Set();
            }
        }

        /// <summary>
        ///  Opened orders
        /// </summary>
        /// <param name="sort">Sort by open time or close time</param>
        /// <param name="ascending">Ascending sort</param>
        /// <returns></returns>
        public Order[] GetOpenedOrders(OrderSort sort = OrderSort.OpenTime, bool ascending = true)
        {
            var res = Orders.Opened.Values.ToList();
            if (sort == OrderSort.CloseTime)
                if (ascending)
                    res.Sort((s1, s2) => s1.CloseTime.CompareTo(s2.CloseTime));
                else
                    res.Sort((s1, s2) => s2.CloseTime.CompareTo(s1.CloseTime));
            else if (sort == OrderSort.OpenTime)
                if (ascending)
                    res.Sort((s1, s2) => s1.OpenTime.CompareTo(s2.OpenTime));
                else
                    res.Sort((s1, s2) => s2.OpenTime.CompareTo(s1.OpenTime));
            foreach (var item in res)
            {
                try
                {
                    if (Symbols.Exist(item.Symbol) && item.Commission > 0 && (Symbols.Infos?.Count ?? 0) > 0)
                    {
                        item.Commission = CommissionOrZero(item.Commission, item.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                        if (item.DealInternalIn != null)
                            item.DealInternalIn.Commission = CommissionOrZero(item.DealInternalIn.Commission, item.DealInternalIn.OpenPrice, Symbols.GetInfo(item.Symbol).Digits, GetTickSize(item.Symbol));
                    }
                }catch {}
                
            }
            return res.ToArray();
        }

        /// <summary>
        /// Get opened order by ticket.
        /// </summary>
        /// <param name="ticket">Unique number of the order ticket.</param>
        /// <returns>Return null if order not found, otherwise return order data.</returns>
        public Order GetOpenedOrder(long ticket)
        {
            lock (Orders)
            {
                if (Orders.Opened.TryGetValue(ticket, out var order))
                    return order;
                else
                    return null;
            }
        }


        private void OnConnectThread(object args)
        {
            try
            {
                OnConnectProgress?.Invoke(this, (ConnectEventArgs)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        internal void OnOrderHisotyCall(OrderHistoryEventArgs args)
        {
            ThreadPool.QueueUserWorkItem(OnOrderHisotyThread, args, ProcessEventTimeoutMs);
        }

        private void OnOrderHisotyThread(object args)
        {
            try
            {
                OnOrderHistory?.Invoke(this, (OrderHistoryEventArgs)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }


        DateTime LastCalProfitAndProps;
        bool CalcProfitAndPropsRunning = false;

        internal void OnQuoteCall(Quote quote)
        {
            if (!CalcProfitAndPropsRunning)
                if (DateTime.Now.Subtract(LastCalProfitAndProps).TotalSeconds > 0.25)
                {
                    LastCalProfitAndProps = DateTime.Now;
                    _ = UpdateProfitsTask(quote);
                }
            LastQuoteTime = quote.Time;
            if (GetQuoteEvents.TryGetValue(quote.Symbol, out var evnt))
                evnt.Pulse();
            ThreadPool.QueueUserWorkItem(OnQuoteThread, quote, ProcessEventTimeoutMs);
        }


        private void OnQuoteThread(object args)
        {
            try
            {
                OnQuote?.Invoke(this, (Quote)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        internal void OnSymbolsUpdateCall()
        {
            ThreadPool.QueueUserWorkItem(OnSymbolsUpdateThread, null, ProcessEventTimeoutMs);
        }

        private void OnSymbolsUpdateThread(object args)
        {
            try
            {
                OnSymbolsUpdate?.Invoke(this);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }


        internal void OnOrderUpdateCall(OrderUpdate[] update)
        {
            try
            {
                foreach (var waiter in UpdateWaiters.Keys)
                    foreach (var item in update)
                        waiter(this, item);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            try
            {
                foreach (var item in update)
                    Orders.Api_OnOrderUpdate(this, item);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            //var o = GetOpenedOrders();
            try
            {
                foreach (var item in update)
                    if (item.Order != null)
                        if (RequestIds.TryGetValue(item.Order.Ticket, out var id))
                            item.Order.RequestId = id;
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            try
            {
                foreach (var order in GetOpenedOrders())
                    if (order.OrderType == OrderType.Buy || order.OrderType == OrderType.Sell)
                        Subscribe(order.Symbol);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            ThreadPool.QueueUserWorkItem(OnOrderUpdateThread, update, ProcessEventTimeoutMs);
        }

        private async void OnOrderUpdateThread(object args)
        {
            try
            {
                var update = (OrderUpdate[])args;
                foreach (var item in update)
                {

                    if (item.Type != UpdateType.Cancelling
                        && item.Type != UpdateType.Started
                        && item.Type != UpdateType.Filled
                        && item.Type != UpdateType.Unknown)
                    {
                        await UpdateProfitsTask(null);
                        await CalcMarginAsync(true);
                        OnOrderUpdate?.Invoke(this, item);
                    }
                    else if (EnableAllOrderUpdates)
                        OnOrderUpdate?.Invoke(this, item);
                }
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }


        internal void OnSymbolUpdateCall(SymbolUpdate update)
        {
            ThreadPool.QueueUserWorkItem(OnSymbolUpdateThread, update, ProcessEventTimeoutMs);
        }

        private void OnSymbolUpdateThread(object args)
        {
            try
            {
                OnSymbolUpdate?.Invoke(this, (SymbolUpdate)args);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        internal void OnOrderProgressCall(OrderProgress[] progress)
        {
            try
            {
                foreach (var progr in progress)
                {
                    long ticket = progr.TradeResult.TicketNumber;
                    if (progr.TradeRequest.DealTicket != 0)
                        ticket = progr.TradeRequest.DealTicket;
                    else if (progr.TradeRequest.OrderTicket != 0)
                        ticket = progr.TradeRequest.OrderTicket;
                    if (ticket != 0)
                        if (RequestIds.TryGetValue(ticket, out var id))
                            RequestIds.TryUpdate(ticket, progr.TradeRequest.RequestId, id);
                        else
                            RequestIds.TryAdd(ticket, progr.TradeRequest.RequestId);
                }
                foreach (var waiter in ProgressWaiters.Keys)
                    foreach (var progr in progress)
                        waiter(this, progr);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
            if (OnOrderProgress != null)
                ThreadPool.QueueUserWorkItem(OnOrderProgressThread, progress, ProcessEventTimeoutMs);
        }

        private void OnOrderProgressThread(object args)
        {
            try
            {
                var update = (OrderProgress[])args;
                foreach (var item in update)
                    OnOrderProgress?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                Log.exception(ex, this);
            }
        }

        /// <summary>
        /// Change account password
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="isInvestor">True - investor passsword, False - master password</param>
        public void ChangePassword(string password, bool isInvestor = false)
        {
            if (password.Length > 16)
                password = password.Substring(0, 16);
            byte[] buf = new byte[8 + password.Length * 2 + 2 * 2];
            BitConverter.GetBytes(User).CopyTo(buf, 0);
            Encoding.Unicode.GetBytes(password).CopyTo(buf, 8);
            Encoding.Unicode.GetBytes("MQ").CopyTo(buf, 8 + password.Length * 2);
            MD5Managed md = new MD5Managed();
            md.HashCore(buf, 0, buf.Length);
            md.HashFinal();
            byte[] hash = md.Hash;
            var outBuf = new OutBuf();
            outBuf.ByteToBuffer(0xC);
            if (isInvestor)
                outBuf.ByteToBuffer(1);
            else
                outBuf.ByteToBuffer(0);
            outBuf.Add(hash);
            Connection.SendPacket(0x6B, outBuf, false).Wait();
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void SetConnection(Connection connection, CmdHandler cmdHandler)
        {
            CmdHandler?.StopCmdHandler();
            Connection?.Close();
            Connection = connection;
            CmdHandler = cmdHandler;
        }

        /// <summary>
        /// Check is quote sessions now
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns></returns>
        public bool IsQuoteSession(string symbol)
        {
            var tradeSessionsForWeek = Symbols.Sessions[symbol].Quotes;
            var todaySessions = tradeSessionsForWeek[(int)ServerTime.DayOfWeek];
            foreach (var item in todaySessions)
                if (ServerTime.TimeOfDay.TotalMinutes > item.StartTime && ServerTime.TimeOfDay.TotalMinutes < item.EndTime)
                    return true;
            return false;
        }

        /// <summary>
        /// Check is trade sessions now
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns></returns>
        public bool IsTradeSession(string symbol)
        {
            var tradeSessionsForWeek = Symbols.Sessions[symbol].Trades;
            var todaySessions = tradeSessionsForWeek[(int)ServerTime.DayOfWeek];
            foreach (var item in todaySessions)
                if (ServerTime.TimeOfDay.TotalMinutes > item.StartTime && ServerTime.TimeOfDay.TotalMinutes < item.EndTime)
                    return true;
            return false;
        }


        /// <summary>
        /// Estimeate RoundtripTime to host
        /// </summary>
        /// <param name="nameOrAddress">Host</param>
        /// <returns>RoundtripTime in milliseconds</returns>
        public static int PingHost(string host, int port = 443)
        {
            DateTime start = DateTime.Now;
            using (var client = new System.Net.Sockets.TcpClient())
                client.Connect(host, port);
            int res = (int)(DateTime.Now.Subtract(start).TotalMinutes * 60 * 1000);
            return res * 2;
        }

        /// <summary>
        /// Cluster server list
        /// </summary>
        public ClusterDetails ClusterDetails()
        {
            if (ClusterSummary != null)
            {
                var res = new ClusterDetails() { General = ClusterSummary };
                res.Servers = new Dictionary<string, AddressRec[]>();
                foreach (var item in ClusterMembers)
                    if (!res.Servers.ContainsKey(item.Key.ServerName))
                        res.Servers.Add(item.Key.ServerName, item.Value);
                    else
                        for (int i = 0; i < int.MaxValue; i++)
                            if (!res.Servers.ContainsKey(item.Key.ServerName + " " + i))
                            {
                                res.Servers.Add(item.Key.ServerName + " " + i, item.Value);
                                break;
                            }
                return res;
            }
            else
                return null;
        }

        /// <summary>
        /// Check investor mode
        /// </summary>
        public bool IsInvestor
        {
            get
            {
                if (Account == null)
                    return false;
                int InvestorFlag = 8;
                if ((Account.TradeFlags & InvestorFlag) != 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Trade disables on server for this account
        /// </summary>
        public bool IsTradeDisableOnServer
        {
            get
            {
                if (Account == null)
                    return false;
                int flag = 4;
                if ((Account.TradeFlags & flag) != 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Account not confirmed
        /// </summary>
        public bool IsNotConfirmedAccount
        {
            get
            {
                if (Account == null)
                    return false;
                int flag = 0x200;
                if ((Account.TradeFlags & flag) != 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Is trade allowed
        /// </summary>
        public bool IsTradeAllowed
        {
            get
            {
                return !IsInvestor && !IsTradeDisableOnServer && !IsNotConfirmedAccount && (Symbols.Base.s9C & 0x10) == 0;
            }
        }

        /// <summary>
        /// Download price hsitory.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="from">From time</param>
        /// <param name="to">To time</param>
        /// <param name="timeFrame">Timeframe in minutes</param>
        /// <returns></returns>
        public Bar[] DownloadQuoteHistory(string symbol, DateTime from, DateTime to, int timeFrame)
        {
            DateTime start = from;
            DateTime end = to;
            var res = new SortedDictionary<DateTime, Bar>();
            if (from > ServerTime.AddHours(24))
            {
                var bars = DownloadQuoteHistoryToday(symbol, timeFrame);
                foreach (var item in bars)
                    if (item.Time >= start && item.Time <= end)
                        if (!res.ContainsKey(item.Time))
                            res.Add(item.Time, item);
            }
            else
            {
                var bars = new List<Bar>();
                while (end > start)
                {
                    bars.AddRange(DownloadQuoteHistoryMonth(symbol, end.Year, end.Month, end.Day, 1));
                    end = end.AddMonths(-1);
                }
                bars.Sort((s1, s2) => s1.Time.CompareTo(s2.Time));
                bars = ConvertToTimeframe(bars, timeFrame);
                foreach (var item in bars)
                    if (item.Time >= from && item.Time <= to)
                        if (!res.ContainsKey(item.Time))
                            res.Add(item.Time, item);
            }
            return res.Values.ToArray();
        }

        /// <summary>
        /// Download price hsitory.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public Task<Bar[]> DownloadQuoteHistoryAsync(string symbol, DateTime from, DateTime to, int timeFrame)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        return DownloadQuoteHistory(symbol, from, to, timeFrame);
                    }
                    catch (DoubleRequestException)
                    {
                        await Task.Delay(100);
                    }
                }
            });
        }

        /// <summary>
        /// Mail inbox
        /// </summary>
        public List<MailMessage> Mails => Mail.Messages;

        public void MailBodyRequest(long id)
        {
            Mail.SendMailRequest(id);
        }

        /// <summary>
        /// Calculate qquity history. Function execution can take long period of time because it request price history and order history inside
        /// </summary>
        /// <param name="from"></param>
        /// <param name="api"></param>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public List<EquityPoint> CalculateEquityHistory(DateTime from, MT5API api, EquityTimeframe timeframe, bool excludeSameBars = true)
        {
            var res = new EquityHistory().CalculateEquityHistory(from, api, timeframe, excludeSameBars);
            CalculateDrawdown(res);
            return res;
        }

        internal void CalculateDrawdown(List<EquityPoint> portfolioValues)
        {
            double peakBalance = portfolioValues[0].Balance;
            double peakEquity = portfolioValues[0].Equity;
            foreach (var item in portfolioValues)
            {
                if (item.Balance > peakBalance)
                    peakBalance = item.Balance;
                if (peakBalance > 0)
                    item.BalanceDrawdownRelative = (peakBalance - item.Balance) / peakBalance;
                item.BalanceDrawdownRaw = peakBalance - item.Balance;

                if (item.Equity > peakEquity)
                    peakEquity = item.Equity;
                if (peakEquity > 0)
                    item.EquityDrawdownRelative = (peakEquity - item.Equity) / peakEquity;
                item.EquityDrawdownRaw = peakEquity - item.Equity;
            }
        }

        /// <summary>
        /// Last 100 orders closed during current session
        /// </summary>
        public Order[] ClosedOrders()
        {
            return Orders.Closed.Values.ToArray();
        }
    }

    /// <summary>
	/// Cluster servers list
	/// </summary>
	public class ClusterDetails
    {
        /// <summary>
        /// Cluster general information
        /// </summary>
        public ServerRec General;
        /// <summary>
        /// Server list
        /// </summary>
        public Dictionary<string, AddressRec[]> Servers;
    }

}
