using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using log4net;

using ACE.Server.Network.Managers;
using System.Threading;

namespace ACE.Server.Network
{
    // Reference: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.beginreceivefrom?view=net-7.0
    public class ConnectionListener
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog packetLog = LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), "Packets");

        public Socket Socket { get; private set; }

        public IPEndPoint ListenerEndpoint { get; private set; }

        private readonly uint listeningPort;

        private readonly byte[] buffer = new byte[ClientPacket.MaxPacketSize];

        private readonly IPAddress listeningHost;

        public ConnectionListener(IPAddress host, uint port)
        {
            log.DebugFormat("ConnectionListener ctor, host {0} port {1}", host, port);

            listeningHost = host;
            listeningPort = port;
        }

        public void Start()
        {
            log.DebugFormat("Starting ConnectionListener, host {0} port {1}", listeningHost, listeningPort);

            try
            {
                ListenerEndpoint = new IPEndPoint(listeningHost, (int)listeningPort);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //{
                //    var sioUdpConnectionReset = -1744830452;
                //    var inValue = new byte[] { 0 };
                //    var outValue = new byte[] { 0 };
                //    Socket.IOControl(sioUdpConnectionReset, inValue, outValue);
                //}

                Socket.Bind(ListenerEndpoint);
                Listen();
            }
            catch (Exception exception)
            {
                log.FatalFormat("Network Socket has thrown: {0}", exception.Message);
                ResetSocket();
                RestartListenerAfterDelay();
                Thread.Sleep(3000);
            }
        }

        public void Shutdown()
        {
            log.DebugFormat("Shutting down ConnectionListener, host {0} port {1}", listeningHost, listeningPort);

            if (Socket != null && Socket.IsBound)
                Socket.Close();
        }

        //private void Listen()
        //{
        //    try
        //    {
        //        EndPoint clientEndPoint = new IPEndPoint(listeningHost, 0);
        //        Socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref clientEndPoint, OnDataReceive, Socket);
        //    }
        //    catch (SocketException socketException)
        //    {
        //        log.DebugFormat("ConnectionListener({2}, {3}).Listen() has thrown {0}: {1}", socketException.SocketErrorCode, socketException.Message, listeningHost, listeningPort);
        //        Listen();
        //    }
        //    catch (Exception exception)
        //    {
        //        log.FatalFormat("ConnectionListener({1}, {2}).Listen() has thrown: {0}", exception.Message, listeningHost, listeningPort);
        //    }
        //}

        private void Listen()
        {
            try
            {
                EndPoint clientEndPoint = new IPEndPoint(listeningHost, 0);
                Socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref clientEndPoint, OnDataReceive, Socket);
            }
            catch (SocketException socketException)
            {
                log.DebugFormat("SocketException: {0}", socketException.Message);
                Listen(); // Retry for socket exceptions
            }
            catch (Exception exception)
            {
                log.FatalFormat("Fatal error: {0}", exception.Message);

                // Reset the socket if needed
                ResetSocket();

                // Restart the listener
                RestartListenerAfterDelay();
            }
        }

        private void ResetSocket()
        {
            if (Socket != null)
            {
                Socket.Close();
                // Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // Example socket type
                log.Warn("Socket reset successfully.");
            }
        }

        private void RestartListenerAfterDelay()
        {
            log.Warn("Restarting listener after 10 seconds...");
            Task.Delay(10000).ContinueWith(_ => Start());
        }
        private void OnDataReceive(IAsyncResult result)
        {
            EndPoint clientEndPoint = null;

            try
            {
                clientEndPoint = new IPEndPoint(listeningHost, 0);
                int dataSize = Socket.EndReceiveFrom(result, ref clientEndPoint);

                IPEndPoint ipEndpoint = (IPEndPoint)clientEndPoint;

                // TO-DO: generate ban entries here based on packet rates of endPoint, IP Address, and IP Address Range

                if (packetLog.IsDebugEnabled)
                {
                    byte[] data = new byte[dataSize];
                    Buffer.BlockCopy(buffer, 0, data, 0, dataSize);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Received Packet (Len: {data.Length}) [{ipEndpoint.Address}:{ipEndpoint.Port}=>{ListenerEndpoint.Address}:{ListenerEndpoint.Port}]");
                    sb.AppendLine(data.BuildPacketString());
                    packetLog.DebugFormat("{0}", sb);
                }

                var packet = new ClientPacket();

                if (packet.Unpack(buffer, dataSize))
                    NetworkManager.ProcessPacket(this, packet, ipEndpoint);

                packet.ReleaseBuffer();
            }
            catch (SocketException socketException)
            {
                // If we get "Connection has been forcibly closed..." error, just eat the exception and continue on
                // This gets sent when the remote host terminates the connection (on UDP? interesting...)
                // TODO: There might be more, should keep an eye out. Logged message will help here.
                if (socketException.SocketErrorCode == SocketError.MessageSize ||
                    socketException.SocketErrorCode == SocketError.NetworkReset ||
                    socketException.SocketErrorCode == SocketError.ConnectionReset)
                {
                    log.DebugFormat("ConnectionListener({3}, {4}).OnDataReceieve() has thrown {0}: {1} from client {2}", socketException.SocketErrorCode, socketException.Message, clientEndPoint != null ? clientEndPoint.ToString() : "Unknown", listeningHost, listeningPort);
                }
                else
                {
                    log.FatalFormat("ConnectionListener({3}, {4}).OnDataReceieve() has thrown {0}: {1} from client {2}", socketException.SocketErrorCode, socketException.Message, clientEndPoint != null ? clientEndPoint.ToString() : "Unknown", listeningHost, listeningPort);
                    return;
                }
            }

            if (result.CompletedSynchronously)
                Task.Run(() => Listen());
            else
                Listen();
        }
    }
}
