// Decompiled with JetBrains decompiler
// Type: BRS.ClipboardHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.Reflection;
using UnityEngine;

namespace BRS
{
  public class ClipboardHelper
  {
    private static PropertyInfo propertyInfo_0;

    private static PropertyInfo smethod_0()
    {
      if ((object) ClipboardHelper.propertyInfo_0 == null)
      {
        ClipboardHelper.propertyInfo_0 = typeof (GUIUtility).GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
        if ((object) ClipboardHelper.propertyInfo_0 == null)
          throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
      }
      return ClipboardHelper.propertyInfo_0;
    }

    public static string clipBoard
    {
      get
      {
        return (string) ClipboardHelper.smethod_0().GetValue((object) null, (object[]) null);
      }
      set
      {
        ClipboardHelper.smethod_0().SetValue((object) null, (object) value, (object[]) null);
      }
    }
  }
}
