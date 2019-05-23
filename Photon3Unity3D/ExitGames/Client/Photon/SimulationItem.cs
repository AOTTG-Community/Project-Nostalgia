using System.Diagnostics;

namespace ExitGames.Client.Photon
{
    internal class SimulationItem
    {
        internal readonly Stopwatch stopw;

        public byte[] DelayedData;
        public int TimeToExecute;

        public SimulationItem()
        {
            this.stopw = new Stopwatch();
            this.stopw.Start();
        }

        public int Delay { get; internal set; }
    }
}