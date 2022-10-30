using System;
using System.Net.Http;

namespace HB.FullStack.Common.Utils
{
    public static class HttpUtil
    {
        public static HttpMessageHandler GetPassLocalSSLHttpMessageHandler()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert!.Issuer.Equals("CN=localhost", Globals.Comparison))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
            };
            return handler;
        }
    }
}
