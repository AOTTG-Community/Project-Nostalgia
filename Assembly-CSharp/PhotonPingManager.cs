using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PhotonPingManager
{
    public static int Attempts = 5;
    public static bool IgnoreInitialAttempt = true;
    public static int MaxMilliseconsPerPing = 800;
    private int PingsRunning;
    public bool UseNative;

    [DebuggerHidden]
    public IEnumerator PingSocket(Region region)
    {
        return new c__IteratorB { region = region, f__this = this};
    }

    public static string ResolveHost(string hostName)
    {
        try
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            if (hostAddresses.Length == 1)
            {
                return hostAddresses[0].ToString();
            }
            for (int i = 0; i < hostAddresses.Length; i++)
            {
                IPAddress address = hostAddresses[i];
                if (address != null)
                {
                    string str = address.ToString();
                    if (str.IndexOf('.') >= 0)
                    {
                        return str;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.Log("Exception caught! " + exception.Source + " Message: " + exception.Message);
        }
        return string.Empty;
    }

    public Region BestRegion
    {
        get
        {
            Region region = null;
            int ping = 0x7fffffff;
            foreach (Region region2 in PhotonNetwork.networkingPeer.AvailableRegions)
            {
                UnityEngine.Debug.Log("BestRegion checks region: " + region2);
                if ((region2.Ping != 0) && (region2.Ping < ping))
                {
                    ping = region2.Ping;
                    region = region2;
                }
            }
            return region;
        }
    }

    public bool Done
    {
        get
        {
            return (this.PingsRunning == 0);
        }
    }

    [CompilerGenerated]
    private sealed class c__IteratorB : IEnumerator, IDisposable, IEnumerator<object>
    {
        internal object current;
        internal int PC;
//        internal Region region; //RD:That line was detected as a duplicated variable declaration
        internal PhotonPingManager f__this;
        internal string cleanIpOfRegion__3;
        internal Exception e__8;
        internal int i__5;
        internal int indexOfColon__4;
        internal bool overtime__6;
        internal PhotonPing ping__0;
        internal int replyCount__2;
        internal int rtt__9;
        internal float rttSum__1;
        internal Stopwatch sw__7;
        internal Region region;

        [DebuggerHidden]
        public void Dispose()
        {
            this.PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.PC;
            this.PC = -1;
            switch (num)
            {
                case 0:
                    this.region.Ping = PhotonPingManager.Attempts * PhotonPingManager.MaxMilliseconsPerPing;
                    this.f__this.PingsRunning++;
                    if (PhotonHandler.PingImplementation != typeof(PingNativeDynamic))
                    {
                        this.ping__0 = (PhotonPing) Activator.CreateInstance(PhotonHandler.PingImplementation);
                        break;
                    }
                    UnityEngine.Debug.Log("Using constructor for new PingNativeDynamic()");
                    this.ping__0 = new PingNativeDynamic();
                    break;

                case 1:
                    goto Label_01B9;

                case 2:
                    goto Label_0268;

                case 3:
                    this.PC = -1;
                    goto Label_02B3;

                default:
                    goto Label_02B3;
            }
            this.rttSum__1 = 0f;
            this.replyCount__2 = 0;
            this.cleanIpOfRegion__3 = this.region.HostAndPort;
            this.indexOfColon__4 = this.cleanIpOfRegion__3.LastIndexOf(':');
            if (this.indexOfColon__4 > 1)
            {
                this.cleanIpOfRegion__3 = this.cleanIpOfRegion__3.Substring(0, this.indexOfColon__4);
            }
            this.cleanIpOfRegion__3 = PhotonPingManager.ResolveHost(this.cleanIpOfRegion__3);
            this.i__5 = 0;
            while (this.i__5 < PhotonPingManager.Attempts)
            {
                this.overtime__6 = false;
                this.sw__7 = new Stopwatch();
                this.sw__7.Start();
                try
                {
                    this.ping__0.StartPing(this.cleanIpOfRegion__3);
                }
                catch (Exception exception)
                {
                    this.e__8 = exception;
                    UnityEngine.Debug.Log("catched: " + this.e__8);
                    this.f__this.PingsRunning--;
                    break;
                }
            Label_01B9:
                while (!this.ping__0.Done())
                {
                    if (this.sw__7.ElapsedMilliseconds >= PhotonPingManager.MaxMilliseconsPerPing)
                    {
                        this.overtime__6 = true;
                        break;
                    }
                    this.current = 0;
                    this.PC = 1;
                    goto Label_02B5;
                }
                this.rtt__9 = (int) this.sw__7.ElapsedMilliseconds;
                if ((!PhotonPingManager.IgnoreInitialAttempt || (this.i__5 != 0)) && (this.ping__0.Successful && !this.overtime__6))
                {
                    this.rttSum__1 += this.rtt__9;
                    this.replyCount__2++;
                    this.region.Ping = (int) (this.rttSum__1 / ((float) this.replyCount__2));
                }
                this.current = new WaitForSeconds(0.1f);
                this.PC = 2;
                goto Label_02B5;
            Label_0268:
                this.i__5++;
            }
            this.f__this.PingsRunning--;
            this.current = null;
            this.PC = 3;
            goto Label_02B5;
        Label_02B3:
            return false;
        Label_02B5:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }
    }
}

