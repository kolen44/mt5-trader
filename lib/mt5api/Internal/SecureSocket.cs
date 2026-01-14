using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtapi.mt5
{
    internal class SecureSocket
    {
        private readonly Logger Log;
        internal Socket Sock;

        internal PackDecrypt Decryptor;
        internal PackEncrypt Encryptor;
		internal ClientWebSocket ExLoginWebsocket;

		public SecureSocket()
        {
            Log = new Logger(this);
        }

        public async Task Send(byte[] buf)
        {
            try
            {
                await Sock?.SendAsync(new ArraySegment<byte>(buf), SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new ConnectException("Disconnected: " + ex.Message);
            }
        }


        public async Task<byte[]> Receive(int count)
        {
            byte[] buf = new byte[count];
            int rest = buf.Length;
            while (rest > 0)
            {
                int len;
				if (Sock == null)
					throw new ConnectException("Disconnected");
				try
				{
                    var segment = new ArraySegment<byte>(buf, buf.Length - rest, rest);
                    len = await Sock?.ReceiveAsync(segment, SocketFlags.None);
                }
				catch (Exception ex)
				{
					throw new ConnectException("Disconnected: " + ex.Message);
				}
                if (len == 0)
                    throw new ConnectException("Server closed the socket");
                else
                    rest -= len;
            }
            if (ExLoginWebsocket != null)
                ExLoginWebsocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Binary, true, CancellationToken.None).Wait() ;
			return buf;
        }

        public async Task Connect(string host, int port, CancellationToken cancellation)
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.DualMode = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            await ConnectWithTimeout(socket, host, port, cancellation);
            Sock = socket;
        }

        private async Task ConnectWithTimeout(Socket socket, string host, int port, CancellationToken cancellation)
        {
            var connectTask = socket.ConnectAsync(host, port);
            try
            {
                if (await Task.WhenAny(connectTask, Task.Delay(System.Threading.Timeout.Infinite, cancellation)) == connectTask)
                    return;
                if (!socket.Connected)
                    throw new TimeoutException($"Connection attempt timed out");
            }
            finally
            {
                if (!socket.Connected)
                    socket.Close();
            }
        }

        private async Task ConnectWithTimeoutProxy(ProxySocket socket, string host, int port, CancellationToken cancellation)
        {
            try
            {
                // Run the blocking connect in a background thread
                var connectTask = Task.Run(() =>
                {
                    socket.Connect(host, port);
                }, cancellation);

                // Wait until either connection completes or cancellation is triggered
                await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cancellation));

                // If cancelled, ensure socket is closed
                cancellation.ThrowIfCancellationRequested();

                // Propagate any exception from connectTask (e.g., SocketException)
                await connectTask;

                if (!socket.Connected)
                    throw new TimeoutException($"Connection attempt to {host}:{port} proxy {socket.ProxyEndPoint} failed or timed out.");
            }
            catch (OperationCanceledException)
            {
                if (socket.Connected)
                    socket.Close();
                throw new TimeoutException($"Connection attempt to {host}:{port} proxy {socket.ProxyEndPoint} timed out.");
            }
            catch (Exception)
            {
                if (!socket.Connected)
                    socket.Close();
                throw;
            }
        }


        public async Task Connect(string targetHost, int targetPort, string proxyHost, int proxyPort,
                string proxyUser, string proxyPassword, ProxyTypes type, CancellationToken cancellation)
        {
            ProxySocket socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.DualMode = true;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 30000);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 30000);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            targetHost = targetHost.Trim();
			proxyHost = proxyHost.Trim(); 
			IPAddress ip;
			if (!IPAddress.TryParse(proxyHost, out ip))
				ip = Dns.GetHostEntry(proxyHost).AddressList[0];
			socket.ProxyEndPoint = new IPEndPoint(ip, proxyPort);
			socket.ProxyUser = proxyUser;
            socket.ProxyPass = proxyPassword;
            socket.ProxyType = type;
            await ConnectWithTimeoutProxy(socket, targetHost, targetPort, cancellation);
            Sock = socket;
            Log.trace("Connected to proxy server");
        }

        public void Close()
        {
            try
            {
                Sock?.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
