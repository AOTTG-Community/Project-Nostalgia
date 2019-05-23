// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.NetworkSimulationSet
// Assembly: Photon3Unity3D, Version=4.0.0.4, Culture=neutral, PublicKeyToken=null
// MVID: 45094ECB-AE08-4341-BE26-E524C8B0C677
// Assembly location: D:\Development\Attack on Titan\Assemblies\Photon3Unity3D.dll

using System.Threading;

namespace ExitGames.Client.Photon
{
  public class NetworkSimulationSet
  {
    private bool isSimulationEnabled = false;
    private int outgoingLag = 100;
    private int outgoingJitter = 0;
    private int outgoingLossPercentage = 1;
    private int incomingLag = 100;
    private int incomingJitter = 0;
    private int incomingLossPercentage = 1;
    public readonly ManualResetEvent NetSimManualResetEvent = new ManualResetEvent(false);
    internal PeerBase peerBase;
    private Thread netSimThread;

    protected internal bool IsSimulationEnabled
    {
      get
      {
        return this.isSimulationEnabled;
      }
      set
      {
        lock (this.NetSimManualResetEvent)
        {
          if (!value)
          {
            lock (this.peerBase.NetSimListIncoming)
            {
              foreach (SimulationItem simulationItem in this.peerBase.NetSimListIncoming)
                simulationItem.ActionToExecute();
              this.peerBase.NetSimListIncoming.Clear();
            }
            lock (this.peerBase.NetSimListOutgoing)
            {
              foreach (SimulationItem simulationItem in this.peerBase.NetSimListOutgoing)
                simulationItem.ActionToExecute();
              this.peerBase.NetSimListOutgoing.Clear();
            }
          }
          this.isSimulationEnabled = value;
          if (this.isSimulationEnabled)
          {
            if (this.netSimThread == null)
            {
              this.netSimThread = new Thread(new ThreadStart(this.peerBase.NetworkSimRun));
              this.netSimThread.IsBackground = true;
              this.netSimThread.Name = "netSim" + (object) SupportClass.GetTickCount();
              this.netSimThread.Start();
            }
            this.NetSimManualResetEvent.Set();
          }
          else
            this.NetSimManualResetEvent.Reset();
        }
      }
    }

    public int OutgoingLag
    {
      get
      {
        return this.outgoingLag;
      }
      set
      {
        this.outgoingLag = value;
      }
    }

    public int OutgoingJitter
    {
      get
      {
        return this.outgoingJitter;
      }
      set
      {
        this.outgoingJitter = value;
      }
    }

    public int OutgoingLossPercentage
    {
      get
      {
        return this.outgoingLossPercentage;
      }
      set
      {
        this.outgoingLossPercentage = value;
      }
    }

    public int IncomingLag
    {
      get
      {
        return this.incomingLag;
      }
      set
      {
        this.incomingLag = value;
      }
    }

    public int IncomingJitter
    {
      get
      {
        return this.incomingJitter;
      }
      set
      {
        this.incomingJitter = value;
      }
    }

    public int IncomingLossPercentage
    {
      get
      {
        return this.incomingLossPercentage;
      }
      set
      {
        this.incomingLossPercentage = value;
      }
    }

    public int LostPackagesOut { get; internal set; }

    public int LostPackagesIn { get; internal set; }

    public override string ToString()
    {
      return string.Format("NetworkSimulationSet {6}.  Lag in={0} out={1}. Jitter in={2} out={3}. Loss in={4} out={5}.", (object) this.incomingLag, (object) this.outgoingLag, (object) this.incomingJitter, (object) this.outgoingJitter, (object) this.incomingLossPercentage, (object) this.outgoingLossPercentage, (object) this.IsSimulationEnabled);
    }
  }
}
