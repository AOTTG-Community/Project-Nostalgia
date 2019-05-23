using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
    internal class SocketTcp : IPhotonSocket, IDisposable
    {
        private readonly object syncer = new object();
        private Socket sock;

        public SocketTcp(PeerBase npeer) : base(npeer)
        {
            bool flag = base.ReportDebugOfLevel(DebugLevel.All);
            if (flag)
            {
                base.Listener.DebugReturn(DebugLevel.All, "SocketTcp: TCP, DotNet, Unity.");
            }
            this.PollReceive = false;
        }

        public override bool Connect()
        {
            bool flag = base.Connect();
            bool flag2 = !flag;
            bool result;
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
            return result;
        }

        public override bool Disconnect()
        {
            bool flag = base.ReportDebugOfLevel(DebugLevel.Info);
            if (flag)
            {
                base.EnqueueDebugReturn(DebugLevel.Info, "SocketTcp.Disconnect()");
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

        public void DnsAndConnect()
        {
            try
            {
                IPAddress ipAddress = IPhotonSocket.GetIpAddress(base.ServerAddress);
                bool flag = ipAddress == null;
                if (flag)
                {
                    throw new ArgumentException("Invalid IPAddress. Address: " + base.ServerAddress);
                }
                this.sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.sock.NoDelay = true;
                this.sock.ReceiveTimeout = this.peerBase.DisconnectTimeout;
                this.sock.SendTimeout = this.peerBase.DisconnectTimeout;
                this.sock.Connect(ipAddress, base.ServerPort);
                base.AddressResolvedAsIpv6 = base.IsIpv6SimpleCheck(ipAddress);
                base.State = PhotonSocketState.Connected;
                this.peerBase.OnConnect();
            }
            catch (SecurityException ex)
            {
                bool flag2 = base.ReportDebugOfLevel(DebugLevel.Error);
                if (flag2)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() to '" + base.ServerAddress + "' failed: " + ex.ToString());
                }
                base.HandleException(StatusCode.SecurityExceptionOnConnect);
                return;
            }
            catch (Exception ex2)
            {
                bool flag3 = base.ReportDebugOfLevel(DebugLevel.Error);
                if (flag3)
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() to '" + base.ServerAddress + "' failed: " + ex2.ToString());
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

        public override PhotonSocketError Receive(out byte[] data)
        {
            data = null;
            return PhotonSocketError.NoData;
        }

        public void ReceiveLoop()
        {
            StreamBuffer streamBuffer = new StreamBuffer(base.MTU);
            byte[] array = new byte[9];
            while (base.State == PhotonSocketState.Connected)
            {
                streamBuffer.SetLength(0L);
                try
                {
                    int i = 0;
                    int num = 0;
                    while (i < 9)
                    {
                        try
                        {
                            num = this.sock.Receive(array, i, 9 - i, SocketFlags.None);
                        }
                        catch (SocketException ex)
                        {
                            bool flag = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected && ex.SocketErrorCode == SocketError.WouldBlock;
                            if (flag)
                            {
                                bool flag2 = base.ReportDebugOfLevel(DebugLevel.All);
                                if (flag2)
                                {
                                    base.EnqueueDebugReturn(DebugLevel.All, "ReceiveLoop() got a WouldBlock exception. This is non-fatal. Going to continue.");
                                }
                                continue;
                            }
                            throw;
                        }
                        i += num;
                        bool flag3 = num == 0;
                        if (flag3)
                        {
                            throw new SocketException(10054);
                        }
                    }
                    bool flag4 = array[0] == 240;
                    if (flag4)
                    {
                        base.HandleReceivedDatagram(array, array.Length, true);
                    }
                    else
                    {
                        int num2 = (int)array[1] << 24 | (int)array[2] << 16 | (int)array[3] << 8 | (int)array[4];
                        bool trafficStatsEnabled = this.peerBase.TrafficStatsEnabled;
                        if (trafficStatsEnabled)
                        {
                            bool flag5 = array[5] == 0;
                            bool flag6 = flag5;
                            if (flag6)
                            {
                                this.peerBase.TrafficStatsIncoming.CountReliableOpCommand(num2);
                            }
                            else
                            {
                                this.peerBase.TrafficStatsIncoming.CountUnreliableOpCommand(num2);
                            }
                        }
                        bool flag7 = base.ReportDebugOfLevel(DebugLevel.All);
                        if (flag7)
                        {
                            base.EnqueueDebugReturn(DebugLevel.All, "TCP < " + num2);
                        }
                        streamBuffer.SetCapacityMinimum(num2 - 7);
                        streamBuffer.Write(array, 7, i - 7);
                        i = 0;
                        num2 -= 9;
                        while (i < num2)
                        {
                            try
                            {
                                num = this.sock.Receive(streamBuffer.GetBuffer(), streamBuffer.Position, num2 - i, SocketFlags.None);
                            }
                            catch (SocketException ex2)
                            {
                                bool flag8 = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected && ex2.SocketErrorCode == SocketError.WouldBlock;
                                if (flag8)
                                {
                                    bool flag9 = base.ReportDebugOfLevel(DebugLevel.All);
                                    if (flag9)
                                    {
                                        base.EnqueueDebugReturn(DebugLevel.All, "ReceiveLoop() got a WouldBlock exception. This is non-fatal. Going to continue.");
                                    }
                                    continue;
                                }
                                throw;
                            }
                            streamBuffer.Position += num;
                            i += num;
                            bool flag10 = num == 0;
                            if (flag10)
                            {
                                throw new SocketException(10054);
                            }
                        }
                        base.HandleReceivedDatagram(streamBuffer.ToArray(), streamBuffer.Length, false);
                        bool flag11 = base.ReportDebugOfLevel(DebugLevel.All);
                        if (flag11)
                        {
                            base.EnqueueDebugReturn(DebugLevel.All, "TCP < " + streamBuffer.Length + ((streamBuffer.Length == num2 + 2) ? " OK" : " BAD"));
                        }
                    }
                }
                catch (SocketException ex3)
                {
                    bool flag12 = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected;
                    if (flag12)
                    {
                        bool flag13 = base.ReportDebugOfLevel(DebugLevel.Error);
                        if (flag13)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, "Receiving failed. SocketException: " + ex3.SocketErrorCode);
                        }
                        bool flag14 = ex3.SocketErrorCode == SocketError.ConnectionReset || ex3.SocketErrorCode == SocketError.ConnectionAborted;
                        if (flag14)
                        {
                            base.HandleException(StatusCode.DisconnectByServer);
                        }
                        else
                        {
                            base.HandleException(StatusCode.ExceptionOnReceive);
                        }
                    }
                }
                catch (Exception ex4)
                {
                    bool flag15 = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected;
                    if (flag15)
                    {
                        bool flag16 = base.ReportDebugOfLevel(DebugLevel.Error);
                        if (flag16)
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                            {
                                "Receive issue. State: ",
                                base.State,
                                ". Server: '",
                                base.ServerAddress,
                                "' Exception: ",
                                ex4
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
            bool flag = this.sock == null || !this.sock.Connected;
            PhotonSocketError result;
            if (flag)
            {
                result = PhotonSocketError.Skipped;
            }
            else
            {
                try
                {
                    this.sock.Send(data, 0, length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    bool flag2 = base.State != PhotonSocketState.Disconnecting && base.State > PhotonSocketState.Disconnected;
                    if (flag2)
                    {
                        bool flag3 = base.ReportDebugOfLevel(DebugLevel.Error);
                        if (flag3)
                        {
                            string text = "";
                            bool flag4 = this.sock != null;
                            if (flag4)
                            {
                                text = string.Format(" Local: {0} Remote: {1} ({2}, {3})", new object[]
                                {
                                    this.sock.LocalEndPoint,
                                    this.sock.RemoteEndPoint,
                                    this.sock.Connected ? "connected" : "not connected",
                                    this.sock.IsBound ? "bound" : "not bound"
                                });
                            }
                            base.EnqueueDebugReturn(DebugLevel.Error, string.Format("Cannot send to: {0} ({4}). Uptime: {1} ms. {2} {3}", new object[]
                            {
                                base.ServerAddress,
                                SupportClass.GetTickCount() - this.peerBase.timeBase,
                                base.AddressResolvedAsIpv6 ? " IPv6" : string.Empty,
                                text,
                                ex
                            }));
                        }
                        base.HandleException(StatusCode.Exception);
                    }
                    return PhotonSocketError.Exception;
                }
                result = PhotonSocketError.Success;
            }
            return result;
        }
    }
}