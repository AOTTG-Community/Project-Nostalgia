using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ExitGames.Client.Photon
{
  public class SupportClass
  {
    protected internal static SupportClass.IntegerMillisecondsDelegate IntegerMilliseconds = (SupportClass.IntegerMillisecondsDelegate) (() => Environment.TickCount);

    public static uint CalculateCrc(byte[] buffer, int length)
    {
      uint num1 = uint.MaxValue;
      uint num2 = 3988292384;
      for (int index1 = 0; index1 < length; ++index1)
      {
        byte num3 = buffer[index1];
        num1 ^= (uint) num3;
        for (int index2 = 0; index2 < 8; ++index2)
        {
          if (((int) num1 & 1) != 0)
            num1 = num1 >> 1 ^ num2;
          else
            num1 >>= 1;
        }
      }
      return num1;
    }

    public static List<MethodInfo> GetMethods(Type type, Type attribute)
    {
      List<MethodInfo> methodInfoList = new List<MethodInfo>();
      if (type == null)
        return methodInfoList;
      foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (attribute == null || method.IsDefined(attribute, false))
          methodInfoList.Add(method);
      }
      return methodInfoList;
    }

    public static int GetTickCount()
    {
      return SupportClass.IntegerMilliseconds();
    }

    public static void CallInBackground(Func<bool> myThread)
    {
      SupportClass.CallInBackground(myThread, 100);
    }

    public static void CallInBackground(Func<bool> myThread, int millisecondsInterval)
    {
      new Thread((ThreadStart) (() =>
      {
        while (myThread())
          Thread.Sleep(millisecondsInterval);
      }))
      {
        IsBackground = true
      }.Start();
    }

    public static void WriteStackTrace(Exception throwable, TextWriter stream)
    {
      if (stream != null)
      {
        stream.WriteLine(throwable.ToString());
        stream.WriteLine(throwable.StackTrace);
        stream.Flush();
      }
      else
      {
        Debug.WriteLine(throwable.ToString());
        Debug.WriteLine(throwable.StackTrace);
      }
    }

    public static void WriteStackTrace(Exception throwable)
    {
      SupportClass.WriteStackTrace(throwable, (TextWriter) null);
    }

    public static string DictionaryToString(IDictionary dictionary)
    {
      return SupportClass.DictionaryToString(dictionary, true);
    }

    public static string DictionaryToString(IDictionary dictionary, bool includeTypes)
    {
      if (dictionary == null)
        return "null";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("{");
      foreach (object key in (IEnumerable) dictionary.Keys)
      {
        if (stringBuilder.Length > 1)
          stringBuilder.Append(", ");
        Type type;
        string str;
        if (dictionary[key] == null)
        {
          type = typeof (object);
          str = "null";
        }
        else
        {
          type = dictionary[key].GetType();
          str = dictionary[key].ToString();
        }
        if (typeof (IDictionary) == type || typeof (Hashtable) == type)
          str = SupportClass.DictionaryToString((IDictionary) dictionary[key]);
        if (typeof (string[]) == type)
          str = string.Format("{{{0}}}", (object) string.Join(",", (string[]) dictionary[key]));
        if (includeTypes)
          stringBuilder.AppendFormat("({0}){1}=({2}){3}", (object) key.GetType().Name, key, (object) type.Name, (object) str);
        else
          stringBuilder.AppendFormat("{0}={1}", key, (object) str);
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    [Obsolete("Use DictionaryToString() instead.")]
    public static string HashtableToString(Hashtable hash)
    {
      return SupportClass.DictionaryToString((IDictionary) hash);
    }

    [Obsolete("Use Protocol.Serialize() instead.")]
    public static void NumberToByteArray(byte[] buffer, int index, short number)
    {
      Protocol.Serialize(number, buffer, ref index);
    }

    [Obsolete("Use Protocol.Serialize() instead.")]
    public static void NumberToByteArray(byte[] buffer, int index, int number)
    {
      Protocol.Serialize(number, buffer, ref index);
    }

    public static string ByteArrayToString(byte[] list)
    {
      if (list == null)
        return string.Empty;
      return BitConverter.ToString(list);
    }

    public delegate int IntegerMillisecondsDelegate();

    public class ThreadSafeRandom
    {
      private static readonly Random _r = new Random();

      public static int Next()
      {
        lock (SupportClass.ThreadSafeRandom._r)
          return SupportClass.ThreadSafeRandom._r.Next();
      }
    }
  }
}
