

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace HB.FullStack.Common.Validate
{
    public static class ValidationMethods
    {
        public static bool IsEmail([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Email);
        }

        public static bool IsMobilePhone([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.MobilePhone);
        }

        public static bool IsAllNumber([NotNullWhen(true)] string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Number);
        }

        public static bool IsPositiveNumber([NotNullWhen(true)] string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.PositiveNumber);
        }

        public static bool IsDay([NotNullWhen(true)] string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Day);
        }

        public static bool IsYear([NotNullWhen(true)] string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Year);
        }

        public static bool IsSmsCode([NotNullWhen(true)] string? text, int? smsCodeLength)
        {
            if (string.IsNullOrEmpty(text) || ( smsCodeLength.HasValue &&  text!.Length != smsCodeLength.Value))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Number);
        }

        public static bool IsMonth([NotNullWhen(true)] string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Month);
        }

        public static bool IsTelePhone([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.TelePhone);
        }

        public static bool IsMobileOrTelePhone([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return IsTelePhone(str) || IsMobilePhone(str);
        }

        public static bool IsPassword([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Password);
        }

        public static bool IsLoginName([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.LoginName);
        }

        public static bool IsNickName([NotNullWhen(true)] string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.NickName);
        }

        internal static bool IsUrl([NotNullWhen(true)] string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return Regex.IsMatch(url, RegExpressions.Url);
        }
    }
}

