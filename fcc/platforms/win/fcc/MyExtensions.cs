using System;
using System.Linq;

namespace fcc
{
    public static class MyExtensions
    {
        public static int WordCount(this String str)
        {
            return str.Split(new[] { ' ' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }
        public static bool IsNumberPart(this char str)
        {
            return char.IsDigit(str) || str == '+' || str == '-';
        }

        public static string FirstWord(this String str)
        {
            str = str.Replace("  ", " ").TrimStart(' ','\t').TrimEnd(' ');
            var c = str.Split();
            return c[0];
        }
        public static string LastWord(this String str)
        {
            var c = str.Split();
            return !c.Any() ? null : c[c.Count() - 1];
        }
        public static string Word(this String str,int n)
        {
            var c = str.Split();
            return c.Count() < n ? null : c[n - 1];
        }
        public static string ToHex9(this int n)
        {
            return String.Format("{0:X9}", n);
        }
        public static string ToHex9(this float n)
        {
            
            var b = BitConverter.GetBytes(n);
            string s = null;
            for(var i=3;i>=0;i--)
            {
                s+=string.Format("{0:X2}",b[i]);
            }
            return "0"+s;
        }
        public static bool IsLegalHex(this string s)
        {
            if (s.Length != 9)
                return false;
            return s.Substring(1, 3).Equals("FFF") || s.Substring(1, 3).Equals("000");
        }

        public static bool IsNumber(this string s)
        {
            return char.IsDigit(s[0]);
        }

    }
}