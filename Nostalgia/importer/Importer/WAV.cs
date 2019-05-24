// Decompiled with JetBrains decompiler
// Type: BRS.Importer.WAV
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BRS.Importer
{
  public class WAV
  {
    private static float smethod_0(byte firstByte, byte secondByte)
    {
      return (float) (short) ((int) secondByte << 8 | (int) firstByte) / 32768f;
    }

    private static int smethod_1(byte[] bytes, int offset = 0)
    {
      int num = 0;
      for (int index = 0; index < 4; ++index)
        num |= (int) bytes[offset + index] << index * 8;
      return num;
    }

    private static byte[] smethod_2(string filename)
    {
      return File.ReadAllBytes(filename);
    }

    public float[] LeftChannel { get; internal set; }

    public float[] RightChannel { get; internal set; }

    public int ChannelCount { get; internal set; }

    public int SampleCount { get; internal set; }

    public int Frequency { get; internal set; }

    public WAV(string filename)
      : this(WAV.smethod_2(filename))
    {
    }

    public WAV(byte[] wav)
    {
      this.ChannelCount = (int) wav[22];
      this.Frequency = WAV.smethod_1(wav, 24);
      int index1;
      int index2;
      int num;
      for (index1 = 12; wav[index1] != (byte) 100 || (wav[index1 + 1] != (byte) 97 || wav[index1 + 2] != (byte) 116) || wav[index1 + 3] != (byte) 97; index1 = index2 + (4 + num))
      {
        index2 = index1 + 4;
        num = (int) wav[index2] + (int) wav[index2 + 1] * 256 + (int) wav[index2 + 2] * 65536 + (int) wav[index2 + 3] * 16777216;
      }
      int index3 = index1 + 8;
      this.SampleCount = (wav.Length - index3) / 2;
      if (this.ChannelCount == 2)
        this.SampleCount /= 2;
      this.LeftChannel = new float[this.SampleCount];
      this.RightChannel = this.ChannelCount != 2 ? (float[]) null : new float[this.SampleCount];
      for (int index4 = 0; index3 < wav.Length && index4 < this.LeftChannel.Length; ++index4)
      {
        this.LeftChannel[index4] = WAV.smethod_0(wav[index3], wav[index3 + 1]);
        index3 += 2;
        if (this.ChannelCount == 2)
        {
          this.RightChannel[index4] = WAV.smethod_0(wav[index3], wav[index3 + 1]);
          index3 += 2;
        }
      }
      this.LeftChannel = ((IEnumerable<float>) this.LeftChannel).Take<float>(this.LeftChannel.Length - this.LeftChannel.Length / 100).ToArray<float>();
      if (this.ChannelCount != 2)
        return;
      this.RightChannel = ((IEnumerable<float>) this.RightChannel).Take<float>(this.RightChannel.Length - this.RightChannel.Length / 100).ToArray<float>();
    }

    public override string ToString()
    {
      return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", (object) this.LeftChannel, (object) this.RightChannel, (object) this.ChannelCount, (object) this.SampleCount, (object) this.Frequency);
    }
  }
}
