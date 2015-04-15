using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compiler1._0;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }
        public static bool IsNumberPart(this char str)
        {
            if (char.IsDigit(str) || str == '+' || str == '-')
                return true;
            return false;
        }
        public static string FirstWord(this String str)
        {
            str = str.Replace("  ", " ").TrimStart(' ','\t').TrimEnd(' ');
            var c = str.Split();
            if (c[0] != null)
            {
                return c[0];
            }
            else
            {
                return null;
            }
        }
        public static string LastWord(this String str)
        {
            var c = str.Split();
            if (c.Count<string>() <= 0)
                return null;
            if (c[c.Count<string>()-1] != null)
            {
                return c[c.Count<string>() - 1];
            }
            else
            {
                return null;
            }
        }
        public static string Word(this String str,int n)
        {
            var c = str.Split();
            if (c.Count<string>() < n)
                return null;
            if (c[n - 1] == null)
                return null;
            return c[n - 1];
        }
        public static string ToHex9(this int n)
        {
            return String.Format("{0:X9}", n);
        }
        public static string ToHex9(this float n)
        {
            
            byte[] b = BitConverter.GetBytes(n);
            string s = null;
            for(int i=3;i>=0;i--)
            {
                s+=string.Format("{0:X2}",b[i]);
            }
            return "0"+s;
        }
        public static bool IsLegalHex(this string s)
        {
            if (s.Length != 9)
                return false;
            else if (s.Substring(1, 3).Equals("FFF") || s.Substring(1, 3).Equals("000"))
                return true;
            else
            {
                return false;
            }
        }
        public static bool IsNumber(this string s)
        {
            return char.IsDigit(s[0]);
        }

    }
}