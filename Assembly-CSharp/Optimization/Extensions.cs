using Optimization.Caching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Optimization
{
    internal static class Extensions
    {
        private static Regex stripHEX = new Regex(@"\[([0-9a-f]{6})\]", RegexOptions.IgnoreCase);

        public static string ColorToString(this Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        public static int CountWords(this string s, string s1)
        {
            return (s.Length - s.Replace(s1, "").Length) / s1.Length;
        }

        public static T Find<T>(this IEnumerable<T> ienum, Func<T, bool> func)
        {
            if (ienum == null) return default(T);
            foreach (var idk in ienum)
            {
                if (func(idk))
                {
                    return idk;
                }
            }
            return default(T);
        }

        public static float GetFloat(object obj)
        {
            if (obj is float)
            {
                return (float)obj;
            }
            return 0f;
        }

        public static Color HexToColor(this string hex, byte a = 255)
        {
            if (hex.Length != 6) return Colors.white;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }

        public static bool IsAny<T>(this IEnumerable<T> me, Func<T, bool> state)
        {
            if (me == null) return false;
            foreach (T idk in me)
            {
                if (state(idk)) continue;
                return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            foreach (char c in s) if (c != ' ') return false;
            return true;
        }

        public static string StripHex(this string str)
        {
            return Regex.Replace(str, @"\[([0-9a-f]{6})\]", string.Empty, RegexOptions.IgnoreCase).Replace("[-]", string.Empty);
        }

        public static string StripHTML(this string str)
        {
            return Regex.Replace(str, @"((<(\/|)(color(?(?=\=).*?)>|b>|size.*?>|i>)))", "", RegexOptions.IgnoreCase);
        }

        public static string ToRGBA(this string str)
        {
            if (Regex.IsMatch(str, @"\[([0-9a-zA-Z]{6})\]"))
            {
                str = str.Contains("[-]") ? Regex.Replace(str, @"\[([0-9a-fA-F]{6})\]", "<color=#$1>").Replace("[-]", "</color>") : Regex.Replace(str, @"\[([0-9a-fA-F]{6})\]", "<color=#$1>");
                var c = (short)(str.CountWords("<color=") - str.CountWords("</color>"));
                for (short i = 0; i < c; i++)
                {
                    str += "</color>";
                }
            }
            return str;
        }
    }
}
