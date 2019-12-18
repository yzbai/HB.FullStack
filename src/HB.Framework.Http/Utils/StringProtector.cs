using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Http
{
    public static class StringProtector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dataProtector"></param>
        /// <returns></returns>
        /// <exception cref="EncoderFallbackException"></exception>
        public static string Protect(string token, IDataProtector dataProtector)
        {
            ThrowIf.NullOrEmpty(token, nameof(token));
            ThrowIf.Null(dataProtector, nameof(dataProtector));

            byte[] byteToken = Encoding.UTF8.GetBytes(token);

            byte[] protectedToken = dataProtector.Protect(byteToken);

            return Convert.ToBase64String(protectedToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protectedToken"></param>
        /// <param name="dataProtector"></param>
        /// <returns></returns>
        /// <exception cref="System.FormatException">can cause by Convert.FromBase64String</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">can cause by dataProtector.Unprotect</exception>
        public static string UnProtect(string protectedToken, IDataProtector dataProtector)
        {
            ThrowIf.Null(dataProtector, nameof(dataProtector));

            if (protectedToken.IsNullOrEmpty())
            {
                return string.Empty;
            }

            string padedString = Pad(protectedToken);

            byte[] bytes = Convert.FromBase64String(padedString);

            if (bytes == null)
            {
                return string.Empty;
            }

            byte[] tokenBytes = dataProtector.Unprotect(bytes);

            if (tokenBytes == null)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(tokenBytes);
        }

        private static string Pad(string text)
        {
            var padding = 3 - ((text.Length + 3) % 4);
            if (padding == 0)
            {
                return text;
            }
            return text + new string('=', padding);
        }
    }
}
