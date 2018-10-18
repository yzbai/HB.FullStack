using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HB.Framework.Common.Validate
{
    public static class ValidationMethods
    {
        public static bool IsEmail(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Email);
        }
        
        public static bool IsMobilePhone(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.MobilePhone);
        }

        public static bool IsPositiveNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.PositiveNumber);
        }

        public static bool IsDay(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Day);
        }

        public static bool IsYear(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Year);
        }

        public static bool IsMonth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Month);
        }

        public static bool IsTelePhone(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.TelePhone);
        }

        public static bool IsMobileOrTelePhone(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return IsTelePhone(str) || IsMobilePhone(str);
        }

        public static bool IsPassword(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Password);
        }


        public static bool IsLoginName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.LoginName);
        }

        public static bool IsUserName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.NickName);
        }

        internal static bool IsUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return Regex.IsMatch(url, RegExpressions.Url);
        }
    }
}
