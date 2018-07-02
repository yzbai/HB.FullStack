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
                throw new ArgumentNullException();
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
    }
}
