using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
    internal class SocketUdp : IPhotonSocket
    {
        private readonly object syncer = new object();

        private Socket sock;

        public SocketUdp(PeerBase npeer) : base(npeer)
        {
            if (base.ReportDebugOfLevel(DebugLevel.All))
            {
                base.Listener.DebugReturn(DebugLevel.All, "CSharpSocket: UDP, Unity3d.");
            }
            base.Protocol = ConnectionProtocol.Udp;
            this.PollReceive = false;
        }

        internal void DnsAndConnect()
        {
            try
            {
                object obj = this.syncer;
                lock (obj)
                {
                    this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPAddress ipAddress = IPhotonSocket.GetIpAddress(base.ServerAddress);
                    this.sock.Connect(ipAddress, base.ServerPort);
                    base.State = PhotonSocketState.Connected;
                }
            }
            catch (SecurityException ex)
            {
                if (base.ReportDebugOfLevel(DebugLevel.Error))
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() failed: " + ex.ToString());
                }
                base.HandleException(StatusCode.SecurityExceptionOnConnect);
                return;
            }
            catch (Exception ex2)
            {
                if (base.ReportDebugOfLevel(DebugLevel.Error))
                {
                    base.Listener.DebugReturn(DebugLevel.Error, "Connect() failed: " + ex2.ToString());
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
                if (!base.Connect())
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
            if (base.ReportDebugOfLevel(DebugLevel.Info))
            {
                base.EnqueueDebugReturn(DebugLevel.Info, "CSharpSocket.Disconnect()");
            }
            base.State = PhotonSocketState.Disconnecting;
            object obj = this.syncer;
            lock (obj)
            {
                if (this.sock != null)
                {
                    try
                    {
                        this.sock.Close();
                        this.sock = null;
                    }
                    catch (Exception arg)
                    {
                        base.EnqueueDebugReturn(DebugLevel.Info, "Exception in Disconnect(): " + arg);
                    }
                }
            }
            base.State = PhotonSocketState.Disconnected;
            return true;
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
                    int length = this.sock.Receive(array);
                    base.HandleReceivedDatagram(array, length, true);
                }
                catch (Exception ex)
                {
                    if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
                    {
                        if (base.ReportDebugOfLevel(DebugLevel.Error))
                        {
                            base.EnqueueDebugReturn(DebugLevel.Error, string.Concat(new object[]
                            {
                                "Receive issue. State: ",
                                base.State,
                                " Exception: ",
                                ex
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
                if (!this.sock.Connected)
                {
                    return PhotonSocketError.Skipped;
                }
                try
                {
                    this.sock.Send(data, 0, length, SocketFlags.None);
                }
                catch
                {
                    return PhotonSocketError.Exception;
                }
            }
            return PhotonSocketError.Success;
        }
    }
}