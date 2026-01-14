using static System.Net.WebRequestMethods;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace mtapi.mt5
{
	internal class ExLogin
	{
		private string[] TerminalServers;
		private string[] ProxyServers;
		private MT5API QC;
		Socket MtServerSocket;
		WebSocket TerminalWebSocket;

		internal ExLogin(string[] terminalServers, string[] proxyServers, MT5API qc)
		{
			QC = qc;
			if (proxyServers == null)
				throw new NullReferenceException("Proxies");
			if (terminalServers == null)
				throw new NullReferenceException("Proxies");
			if (proxyServers.Length == 0)
				throw new ArgumentException("Proxies zero len");
			if (terminalServers.Length == 0)
				throw new ArgumentException("Proxies zero len");
			if (proxyServers.Length == 1 && terminalServers.Length == 1)
				if (proxyServers[0] == terminalServers[0])
					throw new ArgumentException("Just one proxy and its equal to terminal");
			TerminalServers = terminalServers;
			ProxyServers = proxyServers;
		}

		internal async Task<ClientWebSocket> Init(Socket mtServerSocket)
		{
			MtServerSocket = mtServerSocket;
			string terminal = TerminalServers[new Random().Next(TerminalServers.Length)];
			string proxy = ProxyServers[new Random().Next(TerminalServers.Length)];
			while (terminal == proxy)
				proxy = ProxyServers[new Random().Next(TerminalServers.Length)];
			var ws = new ClientWebSocket();
			var cts = new CancellationTokenSource();
			await ws.ConnectAsync(new Uri($"ws://{proxy}/websocket/listen-mt5terminal"), CancellationToken.None);
			byte[] buf = new byte[1];
			var res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
			if (res.Count != 1)
				throw new Exception("Cannot get proxy first reply");
			ushort port;
			if (buf[0] == 0)
			{
				buf = new byte[2];
				res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), cts.Token);
				if (res.Count != 2)
					throw new Exception("Cannot get proxy port reply");
				port = BitConverter.ToUInt16(buf, 0);
			}
			else
			{
				buf = new byte[4];
				res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), cts.Token);
				if (res.Count != 4)
					throw new Exception("Cannot get proxy exception message length");
				var len = BitConverter.ToInt32(buf, 0);
				buf = new byte[len];
				res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), cts.Token);
				if (res.Count != len)
					throw new Exception("Cannot get proxy exception message length");
				throw new Exception(Encoding.UTF8.GetString(buf));
			}
			var str = new Http().DownloadString($"http://{terminal}/mt5/Connect?user={QC.User}&password={QC.Password}&host={proxy}&port={port}&isMT4=false", 5000);
			if (str != "OK")
				throw new Exception(str);
			TerminalWebSocket = ws;
			return ws;
		}

		public void TransferPacketFromTerminalToMtServer()
		{
			var buf = new byte[1024];
			var res = TerminalWebSocket.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None).Result;
			if (res.CloseStatus.HasValue)
				throw new Exception("Connection with proxy server lost");
			//MtServerSocket.Send(buf[0..res.Count]);
		}
	}
}