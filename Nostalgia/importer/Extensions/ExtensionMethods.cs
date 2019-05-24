// Decompiled with JetBrains decompiler
// Type: BRS.Extensions.ExtensionMethods
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BRS.Extensions
{
  public static class ExtensionMethods
  {
    public static bool AreAnyPlaying(this Animation anim, params string[] names)
    {
      return ((IEnumerable<string>) names).Any<string>(new Func<string, bool>(anim.IsPlaying));
    }

    public static void SetSpeed(this Animation anim, float speed)
    {
      foreach (AnimationState animationState in anim)
        animationState.speed = speed;
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
      System.Type type = comp.GetType();
      if ((object) type != (object) other.GetType())
        return default (T);
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      foreach (PropertyInfo property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (property.CanWrite)
        {
          try
          {
            property.SetValue((object) comp, property.GetValue((object) other, (object[]) null), (object[]) null);
          }
          catch
          {
          }
        }
      }
      foreach (FieldInfo field in type.GetFields(bindingAttr))
        field.SetValue((object) comp, field.GetValue((object) other));
      return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
      return go.AddComponent<T>().GetCopyOf<T>(toAdd);
    }

    public static Transform FindChild(this GameObject go, string name)
    {
      foreach (Transform componentsInChild in go.GetComponentsInChildren(typeof (Transform), true))
      {
        if (componentsInChild.name == name)
          return componentsInChild;
      }
      return (Transform) null;
    }
  }
}
