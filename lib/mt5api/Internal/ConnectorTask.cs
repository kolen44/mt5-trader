using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace mtapi.mt5
{
    class ConnectHelper
    {
        private Task _task;
        public Exception Exception = null;
        public bool Timeout = false;
        public ConnectLogs Process = new ConnectLogs();
        MT5API QC;
        internal Connection Connection;
        internal CmdHandler CmdHandler;

        public ConnectHelper(MT5API api)
        {
            QC = api;
            //_task = new Task(async () =>
            //{
            //    try
            //    {
            //        await ConnectToAccount(cancellation);
            //    }
            //    catch (Exception ex)
            //    {
            //        Exception = ex;
            //    }
            //});
        }

        private async Task Connect(CancellationToken cancellation)
        {
            try
            {
                await ConnectToAccount(cancellation);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }


        // Thread methods / properties
        public void Start(CancellationToken cancellation)
        {
            _task = Connect(cancellation);
        }
        //public async Task Join() => await _task;
        //public async Task<bool> Join(int timeout)
        //{
        //    if (await Task.WhenAny(_task, Task.Delay(timeout)) == _task)
        //        return true;
        //    Timeout = true;
        //    Connection?.Close();
        //    CmdHandler?.StopCmdHandler();
        //    Process.Progress += ", timeout";
        //    return false;
        //}

        public async Task<bool> Join(CancellationToken cancellation)
        {
            if (await Task.WhenAny(_task, Task.Delay(System.Threading.Timeout.Infinite, cancellation)) == _task)
                return true;
            Timeout = true;
            Connection?.Close();
            CmdHandler?.StopCmdHandler();
            Process.Progress += ", timeout";
            return false;
        }
        public bool IsAlive => !(_task.IsCompleted || _task.IsCanceled || _task.IsFaulted);

        private async Task ConnectToAccount(CancellationToken cancellation)
        {
            Process.Progress += "ConnectThread started";
            var Connection = new Connection(QC);
            await Connection.Login(false, QC.Log, Process, cancellation);       // login
            Process.Progress += ", login done";
            var CmdHandler = new CmdHandler(QC, Connection);
            Process.Progress += ", cmd handler started";
            Process.Progress += ", waiting account info";
            while (CmdHandler.GotAccountInfo == false)
            {
                if (CmdHandler.AccountLoaderException != null)
                    throw CmdHandler.AccountLoaderException;
                await Task.Delay(100);
            }
            Symbols.AddInstance(QC?.ClusterSummary?.ServerName, QC?.Symbols);
            QC.RequestIds.Clear();
            await Connection.SendPacket(0xA, new OutBuf());
            Process.Progress += ", got account info";
            QC.SetConnection(Connection, CmdHandler);
            QC.ConnectTime = DateTime.Now;
        }
    }

    class ConnectorTask
    {
        MT5API QC;
        ConnectHelper ConnectHelper;
        Logger Log;
        SemaphoreSlim ConnectLock = new SemaphoreSlim(1);

        public ConnectorTask(MT5API qc)
        {
            QC = qc;
            Log = qc.Log;
        }

        public async Task run(string host, int port, CancellationToken cancellation)
        {
            host = host.Trim();
            if (QC.DisallowLocalConnections)
                if (host.StartsWith("192.168.")
                || host.StartsWith("10."))
                    throw new ConnectException("Local network connections not allowed");
            while (true)
            {
                if (QC.Connected)
                    return;
                var helper = ConnectHelper;
                if (helper != null)
                    if (helper.IsAlive)
                        if (helper.Timeout == false)
                            if (await helper.Join(cancellation))
                                if (helper.Exception != null)
                                    throw helper.Exception;
                                else
                                    return;
                break;
            }
            try
            {
                await ConnectLock.WaitAsync(cancellation);
            }
            catch (Exception)
            {
                Exception ex = new TimeoutException("Timeout exception ConnectLock await. Process: " + ConnectHelper.Process.Progress);
                QC.OnConnectCall(ex, ConnectProgress.Exception);
                throw ex;
            }
            try
            {
                if (QC.Connected)
                    return;
                QC.Host = host;
                QC.Port = port;
                ConnectHelper = new ConnectHelper(QC);
                QC.CmdHandler?.StopCmdHandler();
                QC.Connection?.Close();
                ConnectHelper.Start(cancellation);
                if (await ConnectHelper.Join(cancellation))
                {
                    if (ConnectHelper.Exception == null)
                    {
                        var subscriptions = new List<string>(QC.Subscriptions());
                        foreach (var order in QC.GetOpenedOrders())
                            if (order.OrderType == OrderType.Buy || order.OrderType == OrderType.Sell)
                                if (!subscriptions.Contains(order.Symbol))
                                    subscriptions.Add(order.Symbol);
                        if (subscriptions.Count > 0)
                            await QC.Subscriber.SubscribeForce(subscriptions.ToArray()); //in case of reconnection
                        _ = QC.UpdateProfitsTask();
                        _ = QC.CalcMarginAsync(false);
                        QC.OnConnectCall(null, ConnectProgress.Connected);
                    }
                    else
                    {
                        Exception ex;
                        if (ConnectHelper.Exception.InnerException != null)
                        {
                            ex = ConnectHelper.Exception.InnerException;
                            ex.Data["OriginalException"] = ConnectHelper.Exception.ToString();
                        }
                        else
                            ex = ConnectHelper.Exception;
                        if (!(ex is ServerException))
                            ex.SetMessage(ex.Message + ". Process: " + ConnectHelper.Process.Progress);
                        QC.OnConnectCall(ex, ConnectProgress.Exception);
                        throw ex;
                    }
                }
                else
                {
                    Exception ex = new TimeoutException("Not connected because of timeout. Process: " + ConnectHelper.Process.Progress);
                    QC.OnConnectCall(ex, ConnectProgress.Exception);
                    throw ex;
                }
            }
            finally
            {
                ConnectLock.Release();
            }
        }

    }
}
