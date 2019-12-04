using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Http
{
    public class StringProtector
    {
        public static string Protect(string token, IDataProtector dataProtector)
        {
            ThrowIf.NullOrEmpty(token, nameof(token));
            ThrowIf.Null(dataProtector, nameof(dataProtector));

            byte[] byteToken = Encoding.UTF8.GetBytes(token);

            byte[] protectedToken = dataProtector.Protect(byteToken);

            return Convert.ToBase64String(protectedToken);
        }

        public static string UnProtect(string protectedToken, IDataProtector dataProtector)
        {
            ThrowIf.Null(dataProtector, nameof(dataProtector));

            try
            {
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
            catch (Exception)
            {
                return string.Empty;
            }
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
