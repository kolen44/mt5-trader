
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Globalization;

namespace mtapi.mt5
{
    internal class CmdHandler
    {
        private readonly MT5API QuoteClient;
        private readonly Logger Log;
        internal readonly Thread Thread;


        internal bool Stop = false;

        private DateTime LastPing;

        public CmdHandler(MT5API quoteClient, Connection con)
        {
            QuoteClient = quoteClient;
            Log = quoteClient.Log;
            LastPing = DateTime.Now;
            Thread = new Thread(new ParameterizedThreadStart(Run))
            {
                Name = "QuoteCmdHandler",
                IsBackground = true
            };
            Thread.Start(con);
        }


        public bool Running
        {
            get
            {
                return Thread.IsAlive && Stop == false;
            }
        }

        Dictionary<ushort, MemoryStream> Packets = new Dictionary<ushort, MemoryStream>();
        internal bool GotAccountInfo = false;
        internal DateTime GotAccountInfoTime = new DateTime();
        internal Exception AccountLoaderException = null;
        readonly ushort PackCompress = 1;
        readonly ushort PackComplete = 2;
        InBufAccLoad BufferAccountLoader;

        private void Run(object obj)
        {
            Connection con = (Connection)obj;
            try
            {
                while (!Stop)
                {
                    InBuf buf = null;
                    var task = con.RecievePacket();
                    while (!Stop)
                    {
                        if (task.Wait(100))
                        {
                            buf = task.Result;
                            break;
                        }
                    }
                    if (Stop)
                        break;
                    if (buf == null)
                        throw new NullReferenceException("Buf is null");
                    if ((buf.Hdr.Flags & PackCompress) > 0)
                        con.Decompress(buf);
                    if (Packets.ContainsKey(buf.Hdr.Id))
                    {
                        if ((buf.Hdr.Flags & PackComplete) > 0)
                        {
                            if (buf.Hdr.Type == 0xC)
                                BufferAccountLoader.AddBuffer(buf.ToBytes());
                            else
                            {
                                Packets[buf.Hdr.Id].Write(buf.ToBytes(), 0, buf.ToBytes().Length);
                                buf.SetBuf(Packets[buf.Hdr.Id].ToArray());
                            }
                            Clear(Packets[buf.Hdr.Id]);
                            Packets.Remove(buf.Hdr.Id);
                        }
                        else
                        {
                            if (buf.Hdr.Type == 0xC)
                                BufferAccountLoader.AddBuffer(buf.ToBytes());
                            else
                                Packets[buf.Hdr.Id].Write(buf.ToBytes(), 0, buf.ToBytes().Length);
                            continue;
                        }
                    }
                    else
                    {
                        if ((buf.Hdr.Flags & PackComplete) > 0)
                        {

                        }
                        else
                        {
                            Packets.Add(buf.Hdr.Id, new MemoryStream());

                            if (buf.Hdr.Type == 0xC)
                            {
                                BufferAccountLoader = new InBufAccLoad(buf.ToBytes(), buf.Hdr, QuoteClient.ConnectTimeout);
                                new Task(() =>
                                {
                                    try
                                    {
                                        new AccountLoader(QuoteClient, this, con).Parse(BufferAccountLoader);
                                        QuoteClient.OnSymbolsUpdateCall();
                                    }
                                    catch (Exception ex)
                                    {
                                        AccountLoaderException = ex;
                                    }
                                }).Start();
                            }
                            else
                                Packets[buf.Hdr.Id].Write(buf.ToBytes(), 0, buf.ToBytes().Length);
                            continue;
                        }
                    }
                    QuoteClient.LastServerMessageTime = DateTime.Now;
                    var cmd = buf.Hdr.Type;
                    bool needReconnect = false;
                    try
                    {
                        switch (cmd)
                        {
                            case 0x65:
                                Log.trace("Cmd TradeHistory");
                                QuoteClient.OrderHistory.Parse(buf);
                                break;
                            case 0x66:
                                Log.trace("Cmd QuoteHistory");
                                new QuoteHistory(QuoteClient).Parse(buf);
                                break;
                            case 0xC:
                                Log.trace("Cmd AccountLoad finish");
                                BufferAccountLoader.LoadFinish = true;
                                break;
                            case 0x32:
                                Log.trace("ParseTicks");
                                QuoteClient.Subscriber.Parse(buf);
                                break;
                            case 0x36:
                                Log.trace("RecieveMail");
                                QuoteClient.Mail.Parse(buf);
                                break;
                            case 0x37:
                                Log.trace("ParseTrades");
                                needReconnect = QuoteClient.Orders.ParseTrades(buf);
                                break;
                            case 0x38:
                                Log.trace("UpdateQuoteHistory");
                                break;
                            case 0x33:
                                Log.trace("ParseSubscribeSymbols");
                                QuoteClient.Subscriber.ParseSymbolData(buf);
                                break;
                            case 0x34:
                                QuoteClient.OrderBook.ParseBooks(buf);
                                break;
                            case 0xA:
                                Log.trace("Cmd Ping");
                                break;
                            case 0x6C:
                                Log.trace("Cmd TradeResult");
                                ParseResult(buf);
                                break;
                            case 0x68:
                                Log.trace("RecieveMailBody");
                                QuoteClient.Mail.ParseBody(buf);
                                break;
                            case 0x69:
                                Log.trace("Cmd Symbols");
                                new TickParser(QuoteClient).Parse(buf);
                                break;
                            default:
                                Log.trace("Unknown cmd = " + cmd.ToString("X"));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.warn(ex, QuoteClient);
                    }
                    finally
                    {
                        buf.Hdr = null;
                        Array.Resize(ref buf.Buf, 0);
                        buf.Buf = null;
                    }
                    var qc = QuoteClient;
                    if (qc != null)
                        if (qc.DisconnectOnSymbolUpdate)
                            if (needReconnect)
                                if(DateTime.Now.Subtract(GotAccountInfoTime).TotalSeconds > 30)
                                    throw new Exception("Reconnect required after symbol update");
                    if (DateTime.Now.Subtract(LastPing).Seconds > 10)
                    {
                        con.SendPacket(0xA, new OutBuf()).Wait();
                        LastPing = DateTime.Now;
                    }
                }
                Stop = true;
                con.Disconnect();
            }
            catch (Exception ex)
            {
                AccountLoaderException = ex;
                try
                {
                    Log.warn(ex, QuoteClient);
                    Stop = true;
                    QuoteClient.OnDisconnect(ex);
                    con.Close();
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                Symbols.RemoveInstance(QuoteClient?.ClusterSummary?.ServerName, QuoteClient?.Symbols);
            }
        }

        static void Clear(MemoryStream ms)
        {
            var buffer = ms.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            ms.Position = 0;
            ms.SetLength(0);
            ms.Capacity = 0;
            ms.Dispose();
        }

        internal void StopCmdHandler()
        {
            Stop = true;
        }

        private void ParseResult(InBuf buf)
        {
            Msg status = (Msg)buf.Int();
        }
    }

}
