using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
    internal class SocketUdp : IPhotonSocket, IDisposable
    {
        private readonly object syncer = new object();
        private Socket sock;

        public SocketUdp(PeerBase npeer) : base(npeer)
        {
            bool flag = base.ReportDebugOfLevel(DebugLevel.All);
            if (flag)
            {
                base.Listener.DebugReturn(DebugLevel.All, "SocketUdp: UDP, Unity3d.");
            }
            this.PollReceive = false;
        }

        internal void DnsAndConnect()
        {
            IPAddress ipaddress = null;
            try
            {
                ipaddress = IPhotonSocket.GetIpAddress(base.ServerAddress);
                bool flag = ipaddress == null;
                if (flag)
                {
                    throw new ArgumentException("Invalid IPAddress. Address: " + base.ServerAddress);
                }
                object obj = this.syncer;
                lock (obj)
                {
                    bool flag2 = base.State == PhotonSocketState.Disconnecting || base.State == PhotonSocketState.Disconnected;
                    if (flag2)
                    {
                        return;
                    }
                    this.sock = new Socket(ipaddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    this.sock.Connect(ipaddress, base.ServerPort);
                    base.AddressResolvedAsIpv6 = base.IsIpv6SimpleCheck(ipaddress);
                    base.State = PhotonSocketState.Connected;
                    this.peerBase.OnConnect();
                }
            }
            catch (SecurityException ex)
            {
                bool flag3 = base.ReportDebugOfLevel(DebugLevel.Error);
                if (flag3)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, string.Concat(new string[]
                    {
                        "Connect() to '",
                        base.ServerAddress,
                        "' (",
                        (ipaddress == null) ? "" : ipaddress.AddressFamily.ToString(),
                        ") failed: ",
                        ex.ToString()
                    }));
                }
                base.HandleException(StatusCode.SecurityExceptionOnConnect);
                return;
            }
            catch (Exception ex2)
            {
                bool flag4 = base.ReportDebugOfLevel(DebugLevel.Error);
                if (flag4)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, string.Concat(new string[]
                    {
                        "Connect() to '",
                        base.ServerAddress,
                        "' (",
                        (ipaddress == null) ? "" : ipaddress.AddressFamily.ToString(),
                        ") failed: ",
                        ex2.ToString()
                    }));
                }
                base.HandleException(StatusCode.ExceptionOnConnect);
                return;
            }
            new Thread(new ThreadStart(this.ReceiveLoop))
            {
                Name = "photon receive thread",
                IsBackground = true
            }.Start();
        }

        public override bool Connect()
        {
            object obj = this.syncer;
            bool result;
            lock (obj)
            {
                bool flag = base.Connect();
                bool flag2 = !flag;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    base.State = PhotonSocketState.Connecting;
                    new Thread(new ThreadStart(this.DnsAndConnect))
                    {
                        Name = "photon dns thread",
                        IsBackground = true
                    }.Start();
                    result = true;
                }
            }
            return result;
        }

        public override bool Disconnect()
        {
            bool flag = base.ReportDebugOfLevel(DebugLevel.Info);
            if (flag)
            {
                base.EnqueueDebugReturn(DebugLevel.Info, "SocketUdp.Disconnect()");
            }
            base.State = PhotonSocketState.Disconnecting;
            object obj = this.syncer;
            lock (obj)
            {
                bool flag2 = this.sock != null;
                if (flag2)
                {
                    try
                    {
                        this.sock.Close();
                    }
                    catch (Exception arg)
                    {
                        base.EnqueueDebugReturn(DebugLevel.Info, "Exception in Disconnect(): " + arg);
                    }
                    this.sock = null;
                }
            }
            base.State = PhotonSocketState.Disconnected;
            return true;
        }

        public void Dispose()
        {
            base.State = PhotonSocketState.Disconnecting;
            bool flag = this.sock != null;
            if (flag)
            {
                try
                {
                    bool connected = this.sock.Connected;
                    if (connected)
                    {
                        this.sock.Close();
                    }
                }
                catch (Exception arg)
                {
                    base.EnqueueDebugReturn(DebugLevel.Info, "Exception in Dispose(): " + arg);
                }
            }
            this.sock = null;
            base.State = PhotonSocketState.Disconnected;
        }

        public override PhotonSocketError Receive(out byte[] data)
        {
            data = null;
            return PhotonSocketError.NoData;
        }

        public void ReceiveLoop()
        {
            byte[] array = new byte[base.MTU];
            while (base.State == PhotonSocketState.Connected)
            {
                try
                {
                    int length = this.sock.Receive(array, 0, 1200, SocketFlags.None);
                    base.HandleReceivedDatagram(array, length, true);
                }
                catch (SocketException ex)
                {
                    bool flag = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected;
                    if (flag)
                    {
                        bool flag2 = base.ReportDebugOfLevel(DebugLevel.Error);
                        if (flag2)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                            {
                                "Receive issue. State: ",
                                base.State,
                                ". Server: '",
                                base.ServerAddress,
                                "' ErrorCode: ",
                                ex.ErrorCode,
                                " SocketErrorCode: ",
                                ex.SocketErrorCode,
                                " Message: ",
                                ex.Message,
                                " ",
                                ex
                            }));
                        }
                        base.HandleException(StatusCode.ExceptionOnReceive);
                    }
                }
                catch (Exception ex2)
                {
                    bool flag3 = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected;
                    if (flag3)
                    {
                        bool flag4 = base.ReportDebugOfLevel(DebugLevel.Error);
                        if (flag4)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                            {
                                "Receive issue. State: ",
                                base.State,
                                ". Server: '",
                                base.ServerAddress,
                                "' Message: ",
                                ex2.Message,
                                " Exception: ",
                                ex2
                            }));
                        }
                        base.HandleException(StatusCode.ExceptionOnReceive);
                    }
                }
            }
            this.Disconnect();
        }

        public override PhotonSocketError Send(byte[] data, int length)
        {
            object obj = this.syncer;
            lock (obj)
            {
                bool flag = this.sock == null || !this.sock.Connected;
                if (flag)
                {
                    return PhotonSocketError.Skipped;
                }
                try
                {
                    this.sock.Send(data, 0, length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    bool flag2 = base.ReportDebugOfLevel(DebugLevel.Error);
                    if (flag2)
                    {
                        base.EnqueueDebugReturn(DebugLevel.Error, "Cannot send to: " + base.ServerAddress + ". " + ex.Message);
                    }
                    return PhotonSocketError.Exception;
                }
            }
            return PhotonSocketError.Success;
        }
    }
}