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
