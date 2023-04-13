using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.MauiLib.Platforms
{
    public class DebugOnlyHttpMessageHandler : HttpClientHandler
    {
        private string _bypassHost;
        public DebugOnlyHttpMessageHandler(string bypassHost)
        {
            _bypassHost = bypassHost;

            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals($"CN={_bypassHost}", Globals.Comparison))
                {
                    return true;
                }
                else if (cert!.Issuer.Equals($"CN=localhost", Globals.Comparison))
                {
                    return true;
                }
                else if (cert.Issuer.Contains("DO_NOT_TRUST_FiddlerRoot", Globals.Comparison))
                {
                    return true;
                }

                return errors == System.Net.Security.SslPolicyErrors.None;
            };
        }
    }
}
