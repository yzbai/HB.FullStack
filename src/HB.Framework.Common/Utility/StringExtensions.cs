using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Common
{
    public static class StringExtensions
    {
        public static bool IsIn(this string str, params string[] words)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (words == null || words.Length == 0)
            {
                return false;
            }

            foreach (string word in words)
            {
                if (str.Equals(word))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断是否是全大写字母组成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAllUpper(this string str)
        {
            if (str == null || str.Length == 0)
            {
                return false;
            }

            foreach (char c in str)
            {
                if (!Char.IsUpper(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断是否是全小写字母组成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAllLower(this string str)
        {
            if (str == null || str.Length == 0)
            {
                return false;
            }

            foreach (char c in str)
            {
                if (!Char.IsLower(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
