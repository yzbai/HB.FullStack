using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Java.Net;

using Javax.Net.Ssl;

using Xamarin.Android.Net;

namespace HB.FullStack.Client.Maui.Platforms
{
    public class DebugOnlyHttpMessageHandler : HttpClientHandler //AndroidMessageHandler
    {
        private string _bypassHost;
        public DebugOnlyHttpMessageHandler(string bypassHost)
        {
            _bypassHost = bypassHost;

            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals($"CN={_bypassHost}", GlobalSettings.Comparison))
                {
                    return true;
                }
                else if (cert.Issuer.Contains("DO_NOT_TRUST_FiddlerRoot", GlobalSettings.Comparison))
                {
                    return true;
                }

                return errors == System.Net.Security.SslPolicyErrors.None;
            };
        }

        //protected override IHostnameVerifier? GetSSLHostnameVerifier(HttpsURLConnection connection)
        //{
        //    return new AndroidDebugOnlyHostnameVerifier(_bypassHost);
        //    //return base.GetSSLHostnameVerifier(connection);
        //}
    }

    public class AndroidDebugOnlyHostnameVerifier : Java.Lang.Object, Javax.Net.Ssl.IHostnameVerifier
    {
        private readonly string _bypassHost;

        public AndroidDebugOnlyHostnameVerifier(string bypassHost)
        {
            _bypassHost = bypassHost;
        }

        public bool Verify(string? hostname, ISSLSession? session)
        {
            if (hostname == _bypassHost)// && session?.PeerPrincipal?.Name == $"CN={_bypassHost}")
            {
                return true;
            }

            return HttpsURLConnection.DefaultHostnameVerifier!.Verify(hostname, session);
        }
    }
}
