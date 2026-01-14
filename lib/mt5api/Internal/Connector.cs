using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace mtapi.mt5
{
	class ConnectThread
	{
		private Thread Thread;
		public Exception Exception = null;
		public bool Timeout = false;
		public ConnectLogs Process = new ConnectLogs();
		MT5API QC;
		internal Connection Connection;
		internal CmdHandler CmdHandler;

		public ConnectThread(MT5API api, int connectTimeout)
		{
			QC = api;
			Thread = new Thread(() =>
			{
				try
				{
					ConnectToAccount(connectTimeout);
				}
				catch (Exception ex)
				{
					Exception = ex;
				}
			});
		}

		// Thread methods / properties
		public void Start() => Thread.Start();
		public void Join() => Thread.Join();
		public bool Join(int timeout)
        {
			if (Thread.Join(timeout))
				return true;
			Timeout = true;
            Connection?.Close();
            CmdHandler?.StopCmdHandler();
            Process.Progress += ", timeout";
			return false;
		}
		public bool IsAlive => Thread.IsAlive;

		private void ConnectToAccount(int connectTimeout)
		{
			Process.Progress += "ConnectThread started";
			Connection = new Connection(QC);
            var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds((double)connectTimeout)).Token;
            Connection.Login(false, QC.Log, Process, cancellation).Wait();       // login
			Process.Progress += ", login done";
            CmdHandler = new CmdHandler(QC, Connection);
			Process.Progress += ", cmd handler started";
			Process.Progress += ", waiting account info";
			while (CmdHandler.GotAccountInfo == false)
			{
				if (CmdHandler.AccountLoaderException != null)
					throw CmdHandler.AccountLoaderException;
				Thread.Sleep(100);
			}
			Symbols.AddInstance(QC?.ClusterSummary?.ServerName, QC?.Symbols);
            QC.RequestIds.Clear();
			Connection.SendPacket(0xA, new OutBuf()).Wait();
			Process.Progress += ", got account info";
			QC.SetConnection(Connection, CmdHandler);
            QC.ConnectTime = DateTime.Now;
        }
	}

	class Connector
	{
		MT5API QC;
		ConnectThread ConnectThread;
		Logger Log;
		SemaphoreSlim ConnectLock = new SemaphoreSlim(1);

		public Connector(MT5API qc)
		{
			QC = qc;
			Log = qc.Log;
		}

		public void run(int msTimeout, string host, int port, int msTotalTimeout)
		{
			host = host.Trim();
			if(QC.DisallowLocalConnections)
				if (host.StartsWith("192.168.") || host.StartsWith("10."))
					throw new ConnectException("Local network connections not allowed");
			START:
			if (QC.Connected)
				return;
			var thread = ConnectThread;
			if (thread != null)
				if (thread.IsAlive)
					if (thread.Timeout == false)
						if (thread.Join(msTimeout))
							if (thread.Exception != null)
								throw thread.Exception;
							else
								return;
            if (!ConnectLock.Wait(msTimeout))
            {
                Exception ex = new TimeoutException("Not connected in " + msTotalTimeout + " ms. Process: " + ConnectThread.Process.Progress);
                QC.OnConnectCall(ex, ConnectProgress.Exception);
                throw ex;
            }
            try
			{
                if (QC.Connected)
					return;
				thread = ConnectThread;
				if (thread != null)
					if (thread.IsAlive)
						if (thread.Timeout == false)
							goto START;
				QC.Host = host;
				QC.Port = port;
				ConnectThread = new ConnectThread(QC, msTimeout);
				QC.CmdHandler?.StopCmdHandler();
				QC.Connection?.Close();
				ConnectThread.Start();
			}
			finally
			{
				ConnectLock.Release();
			}
			if (ConnectThread.Join(msTimeout))
			{
				if (ConnectThread.Exception == null)
				{
					var subscriptions = new List<string>(QC.Subscriptions());
					foreach (var order in QC.GetOpenedOrders())
						if (order.OrderType == OrderType.Buy || order.OrderType == OrderType.Sell)
							if (!subscriptions.Contains(order.Symbol))
								subscriptions.Add(order.Symbol);
					if (subscriptions.Count > 0)
						QC.Subscriber.SubscribeForce(subscriptions.ToArray()).Wait(); //in case of reconnection
					_ = QC.UpdateProfitsTask();
                    _ = QC.CalcMarginAsync(false);
					QC.OnConnectCall(null, ConnectProgress.Connected);
				}
				else
				{
					Exception ex;
					if (ConnectThread.Exception.InnerException != null)
						ex = ConnectThread.Exception.InnerException;
					else
						ex = ConnectThread.Exception;
					if (!(ex is ServerException))
						ex.SetMessage(ex.Message + ". Process: " + ConnectThread.Process.Progress);
					QC.OnConnectCall(ex, ConnectProgress.Exception);
					throw ex;
				}
			}
			else
			{
				Exception ex = new TimeoutException("Not connected in " + msTotalTimeout + " ms. Process: " + ConnectThread.Process.Progress);
				QC.OnConnectCall(ex, ConnectProgress.Exception);
				throw ex;
			}
		}
	}
}
