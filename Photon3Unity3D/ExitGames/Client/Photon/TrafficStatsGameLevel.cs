// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.TrafficStatsGameLevel
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  public class TrafficStatsGameLevel
  {
    private int timeOfLastDispatchCall;
    private int timeOfLastSendCall;

    public int OperationByteCount { get; internal set; }

    public int OperationCount { get; internal set; }

    public int ResultByteCount { get; internal set; }

    public int ResultCount { get; internal set; }

    public int EventByteCount { get; internal set; }

    public int EventCount { get; internal set; }

    public int LongestOpResponseCallback { get; internal set; }

    public byte LongestOpResponseCallbackOpCode { get; internal set; }

    public int LongestEventCallback { get; internal set; }

    public byte LongestEventCallbackCode { get; internal set; }

    public int LongestDeltaBetweenDispatching { get; internal set; }

    public int LongestDeltaBetweenSending { get; internal set; }

    [Obsolete("Use DispatchIncomingCommandsCalls, which has proper naming.")]
    public int DispatchCalls
    {
      get
      {
        return this.DispatchIncomingCommandsCalls;
      }
    }

    public int DispatchIncomingCommandsCalls { get; internal set; }

    public int SendOutgoingCommandsCalls { get; internal set; }

    public int TotalByteCount
    {
      get
      {
        return this.OperationByteCount + this.ResultByteCount + this.EventByteCount;
      }
    }

    public int TotalMessageCount
    {
      get
      {
        return this.OperationCount + this.ResultCount + this.EventCount;
      }
    }

    public int TotalIncomingByteCount
    {
      get
      {
        return this.ResultByteCount + this.EventByteCount;
      }
    }

    public int TotalIncomingMessageCount
    {
      get
      {
        return this.ResultCount + this.EventCount;
      }
    }

    public int TotalOutgoingByteCount
    {
      get
      {
        return this.OperationByteCount;
      }
    }

    public int TotalOutgoingMessageCount
    {
      get
      {
        return this.OperationCount;
      }
    }

    internal void CountOperation(int operationBytes)
    {
      this.OperationByteCount += operationBytes;
      ++this.OperationCount;
    }

    internal void CountResult(int resultBytes)
    {
      this.ResultByteCount += resultBytes;
      ++this.ResultCount;
    }

    internal void CountEvent(int eventBytes)
    {
      this.EventByteCount += eventBytes;
      ++this.EventCount;
    }

    internal void TimeForResponseCallback(byte code, int time)
    {
      if (time <= this.LongestOpResponseCallback)
        return;
      this.LongestOpResponseCallback = time;
      this.LongestOpResponseCallbackOpCode = code;
    }

    internal void TimeForEventCallback(byte code, int time)
    {
      if (time <= this.LongestEventCallback)
        return;
      this.LongestEventCallback = time;
      this.LongestEventCallbackCode = code;
    }

    internal void DispatchIncomingCommandsCalled()
    {
      if (this.timeOfLastDispatchCall != 0)
      {
        int num = SupportClass.GetTickCount() - this.timeOfLastDispatchCall;
        if (num > this.LongestDeltaBetweenDispatching)
          this.LongestDeltaBetweenDispatching = num;
      }
      ++this.DispatchIncomingCommandsCalls;
      this.timeOfLastDispatchCall = SupportClass.GetTickCount();
    }

    internal void SendOutgoingCommandsCalled()
    {
      if (this.timeOfLastSendCall != 0)
      {
        int num = SupportClass.GetTickCount() - this.timeOfLastSendCall;
        if (num > this.LongestDeltaBetweenSending)
          this.LongestDeltaBetweenSending = num;
      }
      ++this.SendOutgoingCommandsCalls;
      this.timeOfLastSendCall = SupportClass.GetTickCount();
    }

    public void ResetMaximumCounters()
    {
      this.LongestDeltaBetweenDispatching = 0;
      this.LongestDeltaBetweenSending = 0;
      this.LongestEventCallback = 0;
      this.LongestEventCallbackCode = (byte) 0;
      this.LongestOpResponseCallback = 0;
      this.LongestOpResponseCallbackOpCode = (byte) 0;
      this.timeOfLastDispatchCall = 0;
      this.timeOfLastSendCall = 0;
    }

    public override string ToString()
    {
      return string.Format("OperationByteCount: {0} ResultByteCount: {1} EventByteCount: {2}", (object) this.OperationByteCount, (object) this.ResultByteCount, (object) this.EventByteCount);
    }

    public string ToStringVitalStats()
    {
      return string.Format("Longest delta between Send: {0}ms Dispatch: {1}ms. Longest callback OnEv: {3}={2}ms OnResp: {5}={4}ms. Calls of Send: {6} Dispatch: {7}.", (object) this.LongestDeltaBetweenSending, (object) this.LongestDeltaBetweenDispatching, (object) this.LongestEventCallback, (object) this.LongestEventCallbackCode, (object) this.LongestOpResponseCallback, (object) this.LongestOpResponseCallbackOpCode, (object) this.SendOutgoingCommandsCalls, (object) this.DispatchIncomingCommandsCalls);
    }
  }
}
