using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringValidationExtensions
    {
        public static bool IsEmail([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Email);
        }

        public static bool IsMobilePhone([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.MobilePhone);
        }

        public static bool IsAllNumber([NotNullWhen(true)] this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Number);
        }

        public static bool IsPositiveNumber([NotNullWhen(true)] this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.PositiveNumber);
        }

        public static bool IsDay([NotNullWhen(true)] this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Day);
        }

        public static bool IsYear([NotNullWhen(true)] this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Year);
        }

        public static bool IsSmsCode([NotNullWhen(true)] this string? text, int? smsCodeLength)
        {
            if (string.IsNullOrEmpty(text) || (smsCodeLength.HasValue && text!.Length != smsCodeLength.Value))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Number);
        }

        public static bool IsMonth([NotNullWhen(true)] this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Regex.IsMatch(text, RegExpressions.Month);
        }

        public static bool IsTelePhone([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.TelePhone);
        }

        public static bool IsMobileOrTelePhone([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return IsTelePhone(str) || IsMobilePhone(str);
        }

        public static bool IsPassword([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.Password);
        }

        public static bool IsLoginName([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.LoginName);
        }

        public static bool IsNickName([NotNullWhen(true)] this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, RegExpressions.NickName);
        }

        public static bool IsUrl([NotNullWhen(true)] this string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return Regex.IsMatch(url, RegExpressions.Url);
        }
    }
}

