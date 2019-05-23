// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.TrafficStats
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  public class TrafficStats
  {
    public int PackageHeaderSize { get; internal set; }

    public int ReliableCommandCount { get; internal set; }

    public int UnreliableCommandCount { get; internal set; }

    public int FragmentCommandCount { get; internal set; }

    public int ControlCommandCount { get; internal set; }

    public int TotalPacketCount { get; internal set; }

    public int TotalCommandsInPackets { get; internal set; }

    public int ReliableCommandBytes { get; internal set; }

    public int UnreliableCommandBytes { get; internal set; }

    public int FragmentCommandBytes { get; internal set; }

    public int ControlCommandBytes { get; internal set; }

    internal TrafficStats(int packageHeaderSize)
    {
      this.PackageHeaderSize = packageHeaderSize;
    }

    public int TotalCommandCount
    {
      get
      {
        return this.ReliableCommandCount + this.UnreliableCommandCount + this.FragmentCommandCount + this.ControlCommandCount;
      }
    }

    public int TotalCommandBytes
    {
      get
      {
        return this.ReliableCommandBytes + this.UnreliableCommandBytes + this.FragmentCommandBytes + this.ControlCommandBytes;
      }
    }

    public int TotalPacketBytes
    {
      get
      {
        return this.TotalCommandBytes + this.TotalPacketCount * this.PackageHeaderSize;
      }
    }

    public int TimestampOfLastAck { get; set; }

    public int TimestampOfLastReliableCommand { get; set; }

    internal void CountControlCommand(int size)
    {
      this.ControlCommandBytes += size;
      ++this.ControlCommandCount;
    }

    internal void CountReliableOpCommand(int size)
    {
      this.ReliableCommandBytes += size;
      ++this.ReliableCommandCount;
    }

    internal void CountUnreliableOpCommand(int size)
    {
      this.UnreliableCommandBytes += size;
      ++this.UnreliableCommandCount;
    }

    internal void CountFragmentOpCommand(int size)
    {
      this.FragmentCommandBytes += size;
      ++this.FragmentCommandCount;
    }

    public override string ToString()
    {
      return string.Format("TotalPacketBytes: {0} TotalCommandBytes: {1} TotalPacketCount: {2} TotalCommandsInPackets: {3}", (object) this.TotalPacketBytes, (object) this.TotalCommandBytes, (object) this.TotalPacketCount, (object) this.TotalCommandsInPackets);
    }
  }
}
