// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.HttpBase3
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace ExitGames.Client.Photon
{
  internal class HttpBase3 : PeerBase
  {
    private static readonly byte[] pingData = new byte[5]
    {
      (byte) 240,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0
    };
    private static System.Random _rnd = new System.Random();
    internal static readonly byte[] messageHeader = new byte[9]
    {
      (byte) 251,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 243,
      (byte) 2
    };
    private int _challengId = 0;
    private List<byte[]> incomingList = new List<byte[]>();
    private int _lastSendAck = 0;
    private bool _sendAck = false;
    private InvocationCache _invocationCache = new InvocationCache();
    private int _minConnectionsCount = 2;
    private int _maxConnectionsCount = 4;
    private LinkedList<HttpBase3.AsyncRequestState> _requestCache = new LinkedList<HttpBase3.AsyncRequestState>();
    private int _stateIdBase = -1;
    private MemoryStream outgoingStream = (MemoryStream) null;
    private int _grossErrorCount = 0;
    internal HttpBase3.GetLocalMsTimestampDelegate GetLocalMsTimestamp = (HttpBase3.GetLocalMsTimestampDelegate) (() => Environment.TickCount);
    internal const int TCP_HEADER_LEN = 7;
    private const int MAX_GROSS_ERRORS = 3;
    private string HttpPeerID;
    private string UrlParameters;
    private long lastPingTimeStamp;

    public override string PeerID
    {
      get
      {
        return this.HttpPeerID;
      }
    }

    public bool UseGet { get; set; }

    internal HttpBase3()
    {
      ++PeerBase.peerCount;
      this._challengId = HttpBase3._rnd.Next();
      this.InitOnce();
    }

    internal HttpBase3(IPhotonPeerListener listener)
      : this()
    {
      this.Listener = listener;
    }

    public int currentRequestCount
    {
      get
      {
        return this._requestCache.Count;
      }
    }

    public int totalRequestCount
    {
      get
      {
        return this._stateIdBase;
      }
    }

    internal void Request(byte[] data, string urlParamter)
    {
      this.Request(data, urlParamter, HttpBase3.MessageType.NORMAL);
    }

    internal void Request(byte[] data, string urlParameter, HttpBase3.MessageType type)
    {
      int num = Interlocked.Increment(ref this._stateIdBase);
      this._addAckId(ref urlParameter);
      HttpBase3.AsyncRequestState state = new HttpBase3.AsyncRequestState()
      {
        OutgoingData = data,
        type = type,
        id = num
      };
      this._requestCache.AddLast(state);
      this.Request(state, urlParameter);
    }

    private void Request(HttpBase3.AsyncRequestState state, string urlParamter)
    {
      urlParamter = !this.UseGet ? urlParamter + string.Format("&seq={0}", (object) state.id) : urlParamter + string.Format("&seq={0}&data={1}", (object) state.id, (object) Convert.ToBase64String(state.OutgoingData, Base64FormattingOptions.None));
      if (state.type == HttpBase3.MessageType.DISCONNECT)
        urlParamter += "&dis";
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, string.Format("url for request is {0}", (object) urlParamter));
      string url = this.ServerAddress + urlParamter + this.HttpUrlParameters;
      if (this.UseGet)
        state.Request = new WWW(url);
      else
        state.Request = new WWW(url, this.GetPOSTRequestData(state));
    }

    private void _addAckId(ref string urlParamter)
    {
      if (!this._sendAck)
        return;
      this._sendAck = false;
      urlParamter += "&ack=" + (object) this._lastSendAck;
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, string.Format("ack sent for id {0}, pid={1}, cid={2}", (object) this._lastSendAck, (object) this.HttpPeerID, (object) this._challengId));
    }

    private static void writeLength(byte[] target, int value, int targetOffset)
    {
      target[targetOffset++] = (byte) (value >> 24);
      target[targetOffset++] = (byte) (value >> 16);
      target[targetOffset++] = (byte) (value >> 8);
      target[targetOffset++] = (byte) value;
    }

    private byte[] GetPOSTRequestData(HttpBase3.AsyncRequestState state)
    {
      return state.OutgoingData;
    }

    private static int _getStatusCodeFromResponse(byte[] response, HttpBase3 peer)
    {
      int num = 0;
      if (response.Length >= 4)
        num = num | (int) response[0] << 24 | (int) response[1] << 16 | (int) response[2] << 8 | (int) response[3];
      return num;
    }

    private void _webExceptionHandler(HttpBase3.AsyncRequestState state, WWW request)
    {
      if (state.IsDisconnect)
        return;
      if ((this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting || this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected) && !state.restarting && this.debugOut >= DebugLevel.ERROR)
        this.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Request {0} for pid={1} cid={2} failed with error: {3}", (object) state.id, (object) this.HttpPeerID, (object) this._challengId, (object) request.error));
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
      {
        this.EnqueueErrorDisconnect(StatusCode.ExceptionOnConnect);
      }
      else
      {
        if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
          return;
        if (HttpBase3.gotIgnoreStatus(this.getResponseStatus(request)))
        {
          if (this.debugOut < DebugLevel.ALL)
            return;
          this.Listener.DebugReturn(DebugLevel.ALL, "got statues which we ignore");
        }
        else
          this.EnqueueErrorDisconnect(StatusCode.DisconnectByServer);
      }
    }

    private static bool gotIgnoreStatus(int responseStatus)
    {
      return responseStatus == 404;
    }

    private int getResponseStatus(WWW request)
    {
      int result = -1;
      if (request.error != null)
      {
        int num = request.error.IndexOf(' ');
        if (num != -1)
        {
          string s = request.error.Substring(0, num + 1);
//          s.Trim();
          int.TryParse(s, out result);
        }
        return result;
      }
      this.Listener.DebugReturn(DebugLevel.WARNING, string.Format("failed to parse status {0}", (object) result));
      return -1;
    }

    private void GetResponse(HttpBase3.AsyncRequestState state)
    {
      WWW request = state.Request;
      if (request.error != null)
      {
        this._webExceptionHandler(state, request);
      }
      else
      {
        try
        {
          if (request.bytes != null && request.bytes.Length > 0)
            this.ReceiveIncomingCommands(request.bytes, request.bytes.Length);
          Interlocked.Exchange(ref this._grossErrorCount, 0);
        }
        catch (Exception ex)
        {
          this.Listener.DebugReturn(DebugLevel.ERROR, string.Format("exception: msg - {0}\n Stack\n{1}", (object) ex.StackTrace));
          return;
        }
        this._checkAckCondition();
        state.Request.Dispose();
        state.OutgoingData = (byte[]) null;
      }
    }

    private void _checkAckCondition()
    {
      if (this._requestCache.Count == 0 || this._stateIdBase - this._maxConnectionsCount - this._lastSendAck <= 5)
        return;
      int num = this._requestCache.Min<HttpBase3.AsyncRequestState>((Func<HttpBase3.AsyncRequestState, int>) (request => request.id));
      if (num != int.MaxValue && num - this._lastSendAck > 5)
      {
        this._lastSendAck = num - 2;
        this._sendAck = true;
      }
    }

    internal override int QueuedIncomingCommandsCount
    {
      get
      {
        return 0;
      }
    }

    internal override int QueuedOutgoingCommandsCount
    {
      get
      {
        return 0;
      }
    }

    private void CancelRequests()
    {
      foreach (HttpBase3.AsyncRequestState asyncRequestState in this._requestCache)
        asyncRequestState.Abort();
      this._requestCache.Clear();
    }

    private void Reset()
    {
      this._lastSendAck = 0;
      this._stateIdBase = -1;
      this._sendAck = false;
      this._invocationCache.Reset();
      this.HttpPeerID = "";
      this._requestCache.Clear();
    }

    internal override bool Connect(string serverAddress, string appID)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
      {
        if (this.debugOut >= DebugLevel.WARNING)
          this.Listener.DebugReturn(DebugLevel.WARNING, "Connect() called while peerConnectionState != Disconnected. Nothing done.");
        return false;
      }
      if (string.IsNullOrEmpty(serverAddress) || !serverAddress.StartsWith("http", true, (CultureInfo) null))
      {
        this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() with RHTTP failed. ServerAddress must include 'http://' or 'https://' prefix. Was: " + serverAddress);
        return false;
      }
      this.outgoingStream = new MemoryStream(PeerBase.outgoingStreamBufferSize);
      this.Reset();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
      this.ServerAddress = serverAddress;
      this.UrlParameters = "?init&cid=";
      this.UrlParameters += (string) (object) this._challengId;
      if (appID == null)
        appID = "NUnit";
      HttpBase3 httpBase3 = this;
      string str = httpBase3.UrlParameters + "&app=" + appID;
      httpBase3.UrlParameters = str;
      this.UrlParameters += "&clientversion=4.0.0.0";
      this.UrlParameters += "&protocol=GpBinaryV16";
      this.lastPingTimeStamp = (long) this.GetLocalMsTimestamp();
      this.Request((byte[]) null, this.UrlParameters, HttpBase3.MessageType.CONNECT);
      return true;
    }

    internal override void Disconnect()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
        return;
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
      this.CancelRequests();
      this.Request((byte[]) null, this.UrlParameters, HttpBase3.MessageType.DISCONNECT);
      this.Disconnected();
    }

    private void EnqueueErrorDisconnect(StatusCode statusCode)
    {
      lock (this)
      {
        if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected && this.peerConnectionState != PeerBase.ConnectionStateValue.Connecting)
          return;
        this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
      }
      if (this.debugOut >= DebugLevel.INFO)
        this.Listener.DebugReturn(DebugLevel.INFO, string.Format("pid={0} cid={1} is disconnected", (object) this.HttpPeerID, (object) this._challengId));
      this.EnqueueStatusCallback(statusCode);
      this.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.Disconnected()));
    }

    internal void Disconnected()
    {
      this.InitPeerBase();
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void InitPeerBase()
    {
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
    }

    internal override void FetchServerTimestamp()
    {
    }

    internal override bool DispatchIncomingCommands()
    {
      this.CheckRequests();
      return this.DispatchIncommingActions();
    }

    private bool CheckRequests()
    {
      LinkedListNode<HttpBase3.AsyncRequestState> linkedListNode = this._requestCache.First;
      while (linkedListNode != null)
      {
        LinkedListNode<HttpBase3.AsyncRequestState> node = linkedListNode;
        linkedListNode = linkedListNode.Next;
        if (node.Value.Request.isDone)
        {
          this.GetResponse(node.Value);
          this._requestCache.Remove(node);
        }
        else if (node.Value.IsTimedOut)
        {
          HttpBase3.AsyncRequestState asyncRequestState = node.Value;
          asyncRequestState.Abort();
          if (this.debugOut >= DebugLevel.WARNING)
            this.Listener.DebugReturn(DebugLevel.WARNING, string.Format("Request {0} for pid={1} cid={2} aborted by timeout", (object) asyncRequestState.id, (object) this.HttpPeerID, (object) this._challengId));
          this.EnqueueErrorDisconnect(StatusCode.TimeoutDisconnect);
          this._requestCache.Remove(node);
          return true;
        }
      }
      return true;
    }

    private bool DispatchIncommingActions()
    {
      lock (this.ActionQueue)
      {
        while (this.ActionQueue.Count > 0)
          this.ActionQueue.Dequeue()();
      }
      byte[] incoming;
      lock (this.incomingList)
      {
        if (this.incomingList.Count <= 0)
          return false;
        incoming = this.incomingList[0];
        this.incomingList.RemoveAt(0);
      }
      this.ByteCountCurrentDispatch = incoming.Length + 3;
      if (incoming.Length < 2)
        this.Listener.DebugReturn(DebugLevel.WARNING, string.Format("message has length less then 2. data {0}", (object) SupportClass.ByteArrayToString(incoming)));
      return this.DeserializeMessageAndCallback(incoming);
    }

    private void _sendPing()
    {
      if ((long) this.GetLocalMsTimestamp() - this.lastPingTimeStamp > (long) this.timePingInterval)
      {
        this.lastPingTimeStamp = (long) this.GetLocalMsTimestamp();
        int targetOffset = 1;
        Protocol.Serialize(SupportClass.GetTickCount(), HttpBase3.pingData, ref targetOffset);
        this.Request(HttpBase3.pingData, this.UrlParameters, HttpBase3.MessageType.NORMAL);
      }
      int num = this._minConnectionsCount - this._requestCache.Count;
      if (num <= 0)
        return;
      for (int index = 0; index < num; ++index)
        this.Request((byte[]) null, this.UrlParameters, HttpBase3.MessageType.NORMAL);
    }

    internal override bool SendOutgoingCommands()
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected || this._requestCache.Count >= this._maxConnectionsCount)
        return false;
      if (this.outgoingStream.Length > 0L)
      {
        this.Request(this.outgoingStream.ToArray(), this.UrlParameters);
        this.outgoingStream.SetLength(0L);
        this.outgoingStream.Position = 0L;
      }
      this._sendPing();
      return false;
    }

    private static int _readMessageHeader(BinaryReader br)
    {
      int num = (int) br.ReadByte() << 24 | (int) br.ReadByte() << 16 | (int) br.ReadByte() << 8 | (int) br.ReadByte();
      br.ReadByte();
      br.ReadByte();
      return num;
    }

    internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLen)
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
      {
        if (BitConverter.IsLittleEndian)
          Array.Reverse((Array) inBuff, 4, 4);
        this.HttpPeerID = BitConverter.ToInt32(inBuff, 4).ToString();
        this.UrlParameters = "?pid=" + this.HttpPeerID + "&cid=" + (object) this._challengId;
        this.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
        this.EnqueueActionForDispatch(new PeerBase.MyAction(((PeerBase) this).InitCallback));
      }
      else
      {
        this.timestampOfLastReceive = this.GetLocalMsTimestamp();
        this.bytesIn += (long) (inBuff.Length + 7);
        Array.Reverse((Array) inBuff, 0, 4);
        BinaryReader br = new BinaryReader((Stream) new MemoryStream(inBuff));
        this._invocationCache.Invoke(br.ReadInt32(), (Action) (() => this._parseMessage(inBuff, br)));
      }
    }

    private void _parseMessage(byte[] inBuff, BinaryReader br)
    {
      int length = inBuff.Length;
      using (br)
      {
        using (Stream baseStream = br.BaseStream)
        {
          while (baseStream.Position != baseStream.Length)
          {
            switch (br.ReadByte())
            {
              case 251:
                int count = HttpBase3._readMessageHeader(br) - 7;
                if (count == -1)
                {
                  if (this.debugOut < DebugLevel.ERROR)
                    return;
                  this.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Invalid message header for pid={0} cid={1} and message {2}", (object) this.HttpPeerID, (object) this._challengId, (object) SupportClass.ByteArrayToString(inBuff)));
                  return;
                }
                System.Diagnostics.Debug.Assert(count >= 2);
                byte[] numArray = br.ReadBytes(count);
                if (count < 2)
                  this.Listener.DebugReturn(DebugLevel.WARNING, string.Format("data len is to small. data {0}", (object) SupportClass.ByteArrayToString(inBuff)));
                lock (this.incomingList)
                {
                  this.incomingList.Add(numArray);
                  if (this.incomingList.Count % this.warningSize == 0)
                  {
                    this.EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
                    break;
                  }
                  break;
                }
              case 240:
                this.ReadPingResponse(br);
                break;
              default:
                this.Listener.DebugReturn(DebugLevel.WARNING, "Unknow response from server");
                break;
            }
          }
        }
      }
    }

    private void ReadPingResponse(BinaryReader br)
    {
      int num = (int) br.ReadByte() << 24 | (int) br.ReadByte() << 16 | (int) br.ReadByte() << 8 | (int) br.ReadByte();
      this.lastRoundTripTime = SupportClass.GetTickCount() - ((int) br.ReadByte() << 24 | (int) br.ReadByte() << 16 | (int) br.ReadByte() << 8 | (int) br.ReadByte());
      if (!this.serverTimeOffsetIsAvailable)
        this.roundTripTime = this.lastRoundTripTime;
      this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
      if (this.serverTimeOffsetIsAvailable)
        return;
      this.serverTimeOffset = num + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
      this.serverTimeOffsetIsAvailable = true;
    }

    internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, PeerBase.EgMessageType messageType)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Not connected. PeerState: " + (object) this.peerConnectionState);
        this.Listener.OnStatusChanged(StatusCode.SendError);
        return false;
      }
      byte[] message = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypted);
      if (message == null)
        return false;
      this.outgoingStream.Write(message, 0, message.Length);
      return true;
    }

    internal override byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
    {
      byte[] array;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(HttpBase3.messageHeader, 0, HttpBase3.messageHeader.Length);
        Protocol.SerializeOperationRequest(this.SerializeMemStream, opCode, parameters, false);
        if (encrypt)
        {
          byte[] buffer = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(HttpBase3.messageHeader, 0, HttpBase3.messageHeader.Length);
          this.SerializeMemStream.Write(buffer, 0, buffer.Length);
        }
        array = this.SerializeMemStream.ToArray();
      }
      if (messageType != PeerBase.EgMessageType.Operation)
        array[HttpBase3.messageHeader.Length - 1] = (byte) messageType;
      if (encrypt)
        array[HttpBase3.messageHeader.Length - 1] = (byte) ((uint) array[HttpBase3.messageHeader.Length - 1] | 128U);
      int targetOffset = 1;
      Protocol.Serialize(array.Length, array, ref targetOffset);
      return array;
    }

    internal override void StopConnection()
    {
      throw new NotImplementedException();
    }

    public delegate int GetLocalMsTimestampDelegate();

    internal enum MessageType
    {
      CONNECT,
      DISCONNECT,
      NORMAL,
    }

    private class AsyncRequestState
    {
      public int rerequested = 0;
      public bool restarting = false;
      public int id = 0;
      private Stopwatch Watch;
      public HttpBase3.MessageType type;

      public AsyncRequestState()
      {
        this.Watch = new Stopwatch();
        this.Watch.Start();
      }

      private int ElapsedTime
      {
        get
        {
          return (int) this.Watch.ElapsedMilliseconds;
        }
      }

      public WWW Request { get; set; }

      public byte[] OutgoingData { get; set; }

      public bool IsDisconnect
      {
        get
        {
          return this.type == HttpBase3.MessageType.DISCONNECT;
        }
      }

      public bool IsTimedOut
      {
        get
        {
          return this.ElapsedTime > 10000;
        }
      }

      public void Abort()
      {
        this.Request.Dispose();
        this.Request = (WWW) null;
      }
    }
  }
}
