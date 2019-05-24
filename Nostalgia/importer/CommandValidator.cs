// Decompiled with JetBrains decompiler
// Type: BRS.CommandValidator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BRS
{
  public class CommandValidator
  {
    public readonly string Usage;
    public readonly string Help;
    private readonly List<string> list_0;
    private readonly List<Type> list_1;
    private readonly ushort ushort_0;

    public CommandValidator(string Usage, string Help, List<string> ArgsNames, List<Type> ArgsTypes, ushort RequiredArgs)
    {
      if (ArgsNames.Count != ArgsTypes.Count)
        throw new ArgumentException("Argument names and argument types are of different lengths.");
      if ((int) RequiredArgs > ArgsNames.Count)
        throw new ArgumentException("Required arguments can't be more than the arguments.");
      this.Usage = Usage;
      this.Help = Help;
      this.list_0 = ArgsNames;
      this.list_1 = ArgsTypes;
      this.ushort_0 = RequiredArgs;
    }

    public void CheckArgs(string[] Args)
    {
      int num = Args.Length - 1;
      if (num < (int) this.ushort_0)
        throw new ArgumentException("Not enough arguments were supplied.");
      if (num > this.list_0.Count)
        throw new ArgumentException("Too many arguments were supplied.");
      for (int index = 0; index < num; ++index)
      {
        TypeConverter converter = TypeDescriptor.GetConverter(this.list_1[index]);
        try
        {
          converter.ConvertFromInvariantString(Args[index + 1]);
        }
        catch (Exception ex)
        {
          throw new ArgumentException("Invalid argument " + this.list_0[index] + ": Expected a " + this.list_1[index].Name + " value.");
        }
      }
    }
  }
}
