// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SimulationItem
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System.Diagnostics;

namespace ExitGames.Client.Photon
{
  internal class SimulationItem
  {
    internal readonly Stopwatch stopw;
    public int TimeToExecute;
    public PeerBase.MyAction ActionToExecute;

    public SimulationItem()
    {
      this.stopw = new Stopwatch();
      this.stopw.Start();
    }

    public int Delay { get; internal set; }
  }
}
