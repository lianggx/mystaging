using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MyStaging.Common
{
    public static class _TypeExtension
    {
        public static bool In<T>(this T sender, params T[] _vals)
        {
            return true;
        }

        public static bool NotIn<T>(this T sender, params T[] _vals)
        {
            return true;
        }
        public static bool Like<T>(this T sender, T _val)
        {
            return true;
        }
        public static bool NotLike<T>(this T sender, T vals)
        {
            return true;
        }

        /// <summary>
        ///  将首字母转大写
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToUpperPascal(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string _first = text.Substring(0, 1).ToUpper();
            string _value = text.Substring(1);

            return $"{_first}{_value}";
        }
    }
}
